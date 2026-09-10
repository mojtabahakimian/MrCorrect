using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Data.SqlClient;

namespace Prg_Proccessy.AUDIT
{
    /// <summary>
    /// شنونده‌ی مرکزی دستورهای SQL.
    ///
    /// چرا لازم شد: قلاب‌های قبلی روی ۹ متد دسترسی به داده بودند، ولی بخش
    /// بزرگی از نرم‌افزار مستقیم روی یک <see cref="SqlConnection"/> خام با
    /// Dapper می‌نویسد — ۷۵ نقطه، از جمله ساخت و ویرایش فاکتور و پیش‌فاکتور و
    /// سند (HEAD_LST، INVO_LST، DEED_HED، DEED_DTL). آن نوشتن‌ها از دید
    /// قلاب‌ها نامرئی بودند، یعنی مهم‌ترین اسناد سیستم سابقه نمی‌خوردند.
    ///
    /// Microsoft.Data.SqlClient برای هر دستور رویداد DiagnosticSource منتشر
    /// می‌کند. با شنیدن آن، هر دستوری که از هر مسیری اجرا شود دیده می‌شود و
    /// دیگر لازم نیست هیچ نقطه‌ی فراخوانی‌ای دستکاری شود.
    ///
    /// نکته‌ی حیاتی که با آزمایش کشف شد: <c>SqlTransaction.Rollback()</c> هم
    /// همان کلید رویدادِ <c>WriteTransactionCommitAfter</c> را منتشر می‌کند،
    /// پس نام رویداد به‌تنهایی commit را از rollback جدا نمی‌کند. تنها چیزی
    /// که تفکیک می‌کند فیلد <c>Operation</c> است (<c>Commit</c> در برابر
    /// <c>Rollback</c>). اگر روی نام رویداد تکیه می‌شد، نوشتن‌های برگشت‌خورده
    /// به‌عنوان واقعی ثبت می‌شدند.
    ///
    /// تراکنشی که بدون commit فقط Dispose شود هیچ رویداد پایانی نمی‌دهد؛
    /// چنین چیزی از نظر SQL Server یعنی rollback، پس دور ریخته می‌شود.
    /// </summary>
    internal static class AuditCommandListener
    {
        private const string ListenerName = "SqlClientDiagnosticListener";
        private const string CommandAfter = "Microsoft.Data.SqlClient.WriteCommandAfter";
        private const string TxEnd = "Microsoft.Data.SqlClient.WriteTransactionCommitAfter";
        private const string ConnClosed = "Microsoft.Data.SqlClient.WriteConnectionCloseAfter";

        /// <summary>سقف دستور در یک تراکنش، تا تراکنش خیلی بزرگ حافظه را نبلعد.</summary>
        private const int MaxPerTransaction = 500;

        /// <summary>سقف تراکنش‌های باز هم‌زمان.</summary>
        private const int MaxOpenTransactions = 200;

        private static readonly object _gate = new();
        private static IDisposable? _allListeners;

        // SqlClient بیش از یک DiagnosticListener با همین نام منتشر می‌کند و
        // رویدادها بین آن‌ها پخش می‌شود: در عمل دستورها از یکی می‌آمد و
        // پایان تراکنش از دیگری. با اشتراک روی فقط اولی، پایان تراکنش هرگز
        // دیده نمی‌شد و نوشتن‌های داخل تراکنش بی‌صدا ثبت نمی‌شدند. پس به همه
        // مشترک می‌شویم و برای جلوگیری از اشتراک دوباره روی یک نمونه،
        // نمونه‌های دیده‌شده نگه داشته می‌شوند.
        private static readonly List<IDisposable> _events = new();
        private static readonly HashSet<DiagnosticListener> _seen = new();

        /// <summary>دستورهای منتظرِ سرنوشت تراکنش، کلید: TransactionId.</summary>
        private static readonly ConcurrentDictionary<long, Pending> _pending = new();

        private sealed class Pending
        {
            public long ConnectionHash;
            public readonly List<(string Sql, object? Parameters)> Rows = new();
        }

        internal static bool IsAttached { get; private set; }

        internal static void Attach()
        {
            lock (_gate)
            {
                if (IsAttached) return;
                try
                {
                    _allListeners = DiagnosticListener.AllListeners.Subscribe(new ListenerObserver());
                    IsAttached = true;
                }
                catch (Exception)
                {
                    // شنونده اختیاری است؛ اگر نشد، کار کاربر نباید متوقف شود.
                }
            }
        }

        internal static void Detach()
        {
            lock (_gate)
            {
                foreach (var d in _events)
                {
                    try { d.Dispose(); } catch { }
                }
                _events.Clear();
                _seen.Clear();
                try { _allListeners?.Dispose(); } catch { }
                _allListeners = null;
                IsAttached = false;
                _pending.Clear();
            }
        }

        private sealed class ListenerObserver : IObserver<DiagnosticListener>
        {
            public void OnNext(DiagnosticListener listener)
            {
                if (listener.Name != ListenerName) return;
                try
                {
                    lock (_gate)
                    {
                        // هر نمونه فقط یک بار؛ وگرنه یک دستور چند بار ثبت می‌شد.
                        if (!_seen.Add(listener)) return;
                        _events.Add(listener.Subscribe(new EventObserver()));
                    }
                }
                catch (Exception) { }
            }

            public void OnError(Exception error) { }
            public void OnCompleted() { }
        }

        private sealed class EventObserver : IObserver<KeyValuePair<string, object?>>
        {
            public void OnNext(KeyValuePair<string, object?> evt)
            {
                try
                {
                    switch (evt.Key)
                    {
                        case CommandAfter: OnCommand(evt.Value); break;
                        case TxEnd: OnTransactionEnd(evt.Value); break;
                        case ConnClosed: OnConnectionClosed(evt.Value); break;
                    }
                }
                catch (Exception)
                {
                    // تشخیص سابقه هرگز نباید دستور اصلی کاربر را خراب کند.
                }
            }

            public void OnError(Exception error) { }
            public void OnCompleted() { }
        }

        // ── خواندن فیلدهای payload ───────────────────────────────────────
        // payload یک نوع ناشناس داخلی است، پس فقط با بازتاب خوانده می‌شود.
        // PropertyInfo هر نوع کش می‌شود تا در مسیر داغ بازتاب تکرار نشود.
        private static readonly ConcurrentDictionary<(Type, string), PropertyInfo?> _props = new();

        private static object? Get(object? payload, string name)
        {
            if (payload is null) return null;
            var key = (payload.GetType(), name);
            var pi = _props.GetOrAdd(key, static k => k.Item1.GetProperty(k.Item2));
            return pi?.GetValue(payload);
        }

        private static void OnCommand(object? payload)
        {
            if (Get(payload, "Command") is not SqlCommand cmd) return;

            var sql = cmd.CommandText;
            if (string.IsNullOrWhiteSpace(sql)) return;

            // غربال سریع: بیشتر ترافیک SELECT است و نباید هزینه‌ای بدهد.
            if (!AuditSqlSniffer.LooksLikeWrite(sql)) return;

            var txId = Get(payload, "TransactionId") as long?;

            if (txId is null || txId == 0)
            {
                // بدون تراکنش صریح: موفق شدن دستور یعنی نهایی شده.
                // پارامترها اینجا هم کپی می‌شوند، نه اینکه خودِ
                // SqlParameterCollection پاس داده شود: تحلیل‌گر آن نوع را
                // نمی‌شناسد و مقدارها resolve نمی‌شدند، پس «مقدار جدید فیلد»
                // در DETAIL خالی می‌ماند.
                AuditSqlSniffer.Observe(sql, Snapshot(cmd.Parameters));
                return;
            }

            // داخل تراکنش: تا معلوم شدن سرنوشت نگه داشته می‌شود.
            if (_pending.Count >= MaxOpenTransactions && !_pending.ContainsKey(txId.Value)) return;

            var slot = _pending.GetOrAdd(txId.Value, _ => new Pending());
            lock (slot)
            {
                if (slot.Rows.Count >= MaxPerTransaction) return;
                slot.ConnectionHash = ConnectionKey(cmd.Connection);
                // پارامترها به تراکنش گره خورده‌اند و ممکن است پس از پایان
                // دستور بازاستفاده شوند، پس همین‌جا کپی می‌شوند.
                slot.Rows.Add((sql, Snapshot(cmd.Parameters)));
            }
        }

        private static void OnTransactionEnd(object? payload)
        {
            var txId = Get(payload, "TransactionId") as long?;
            if (txId is null) return;
            if (!_pending.TryRemove(txId.Value, out var slot)) return;

            // نام رویداد برای commit و rollback یکی است؛ فقط Operation فرق دارد.
            var op = Get(payload, "Operation") as string;
            if (!string.Equals(op, "Commit", StringComparison.OrdinalIgnoreCase)) return;

            lock (slot)
            {
                foreach (var (sql, ps) in slot.Rows)
                {
                    AuditSqlSniffer.Observe(sql, ps);
                }
            }
        }

        private static void OnConnectionClosed(object? payload)
        {
            if (Get(payload, "Connection") is not SqlConnection conn) return;
            var key = ConnectionKey(conn);
            if (key == 0) return;

            // تراکنشی که بدون commit بسته شده از نظر SQL Server برگشت خورده،
            // پس دور ریخته می‌شود — وگرنه در حافظه می‌ماند و نشت می‌کرد.
            foreach (var kv in _pending)
            {
                if (kv.Value.ConnectionHash == key) _pending.TryRemove(kv.Key, out _);
            }
        }

        private static long ConnectionKey(SqlConnection? conn)
            => conn is null ? 0 : System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(conn);

        /// <summary>
        /// کپی نام و مقدار پارامترها. خودِ SqlParameterCollection پس از پایان
        /// دستور ممکن است پاک یا بازاستفاده شود، پس نگه داشتنش امن نیست.
        /// </summary>
        private static Dictionary<string, object?>? Snapshot(SqlParameterCollection? ps)
        {
            if (ps is null || ps.Count == 0) return null;
            var d = new Dictionary<string, object?>(ps.Count, StringComparer.OrdinalIgnoreCase);
            foreach (SqlParameter p in ps)
            {
                var name = p.ParameterName?.TrimStart('@');
                if (string.IsNullOrEmpty(name)) continue;
                d[name!] = p.Value is DBNull ? null : p.Value;
            }
            return d;
        }
    }
}
