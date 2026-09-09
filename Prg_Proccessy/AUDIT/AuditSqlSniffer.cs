using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Dapper;

namespace Prg_Proccessy.AUDIT
{
    /// <summary>
    /// تشخیص خودکار عملیات INSERT / UPDATE / DELETE از روی متن دستور SQL.
    ///
    /// چرا این‌طور و نه با قلاب دستی در تک‌تک فرم‌ها:
    /// در کد بیش از ۱۸۰۰ نقطه‌ی نوشتن SQL وجود دارد و دست‌زدن به همه‌ی آن‌ها
    /// نه شدنی است نه امن. راه دیگر — تریگر دیتابیس — هم بسته است، چون ۹۲
    /// دستور از OUTPUT بدون INTO استفاده می‌کنند و SQL Server وجود تریگر روی
    /// آن جدول‌ها را ممنوع می‌کند (خطای ۳۳۴).
    ///
    /// محدودیت‌هایی که باید بدانید:
    ///   • یک دستور ممکن است چند statement داشته باشد؛ همه‌ی آن‌ها تشخیص داده
    ///     می‌شوند، ولی تا سقف <see cref="MaxStatementsPerCommand"/>.
    ///   • مقدار کلید رکورد وقتی دستور پارامتری باشد از خودِ شیء پارامترها
    ///     خوانده می‌شود؛ اگر قابل استخراج نبود، کلید خالی می‌ماند و هرگز
    ///     مقدار بی‌معنی مثل «CODE=@CODE» ذخیره نمی‌شود.
    ///   • این لایه فقط می‌گوید «چه جدولی، چه عملیاتی، روی چه کلیدی». مقدار
    ///     قبل و بعدِ فیلدها را نمی‌دهد؛ برای آن باید به جدول‌های TR_ مراجعه شود.
    /// </summary>
    public static class AuditSqlSniffer
    {
        /// <summary>شیر اصلی. اگر حجم سابقه مشکل‌ساز شد، با false کردن این، تشخیص خودکار خاموش می‌شود.</summary>
        public static bool Enabled { get; set; } = true;

        /// <summary>سقف تعداد statement از یک دستور، تا یک اسکریپت طولانی سیل رویداد راه نیندازد.</summary>
        private const int MaxStatementsPerCommand = 8;

        private static readonly RegexOptions Opts =
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;

        private static readonly Regex RxInsert =
            new(@"\bINSERT\s+INTO\s+(?:\[?dbo\]?\s*\.\s*)?\[?([A-Za-z_][A-Za-z0-9_]*)\]?", Opts);

        private static readonly Regex RxUpdate =
            new(@"\bUPDATE\s+(?:TOP\s*\([^)]*\)\s*)?(?:\[?dbo\]?\s*\.\s*)?\[?([A-Za-z_][A-Za-z0-9_]*)\]?\s+SET\b", Opts);

        // «DELETE d FROM dbo.X» با الگوی ساده نام مستعار را جدول می‌گیرد،
        // پس اول شکل FROM‌دار امتحان می‌شود.
        private static readonly Regex RxDeleteFrom =
            new(@"\bDELETE\b(?:\s+TOP\s*\([^)]*\))?[^;]{0,80}?\bFROM\s+(?:\[?dbo\]?\s*\.\s*)?\[?([A-Za-z_][A-Za-z0-9_]*)\]?", Opts);

        private static readonly Regex RxDeleteBare =
            new(@"\bDELETE\s+(?:TOP\s*\([^)]*\)\s*)?(?:\[?dbo\]?\s*\.\s*)?\[?([A-Za-z_][A-Za-z0-9_]{2,})\]?", Opts);

        /// <summary>ستون‌های کلیدی رایج، به ترتیب اولویت.</summary>
        private static readonly Regex RxKey =
            new(@"\b(N_S|NUMBER1|NUMBER|IDH|IDD|CODE|N_SERI|ID)\s*=\s*(N?'[^']*'|@[A-Za-z_][A-Za-z0-9_]*|[0-9.]+)", Opts);

        private static readonly Regex RxTag =
            new(@"\bTAG\s*=\s*(\d{1,3})\b", Opts);

        /// <summary>امضا: SGN1 = 1 و مانند آن. عمداً SGN1usid را نمی‌گیرد.</summary>
        private static readonly Regex RxSign =
            new(@"\bSGN(\d)\s*=\s*(\d)\b", Opts);

        /// <summary>
        /// جدول‌هایی که ثبت نمی‌شوند. خودِ جدول‌های سابقه حتماً باید اینجا
        /// باشند تا حلقه‌ی بی‌پایان ایجاد نشود.
        /// </summary>
        private static readonly HashSet<string> Excluded = new(StringComparer.OrdinalIgnoreCase)
        {
            "SYS_AUDIT_EVENT", "SYS_AUDIT_SESSION", "AMALIAT", "USER_AUDIT_LOG",
            "UserState", "PAY2_SEC_AUDIT", "CC_RUNLOG", "SMS_SENDS",
        };

        private static readonly Dictionary<string, string> TableLabels = new(StringComparer.OrdinalIgnoreCase)
        {
            ["DEED_HED"] = "سند حسابداری",
            ["DEED_DTL"] = "ردیف سند حسابداری",
            ["PGET_HED"] = "سند خزانه",
            ["PGET_LST"] = "ردیف سند خزانه",
            ["INVO_LST"] = "ردیف برگه",
            ["PAY_GETD"] = "چک دریافتی",
            ["PAY_GETP"] = "چک پرداختی",
            ["STUF_DEF"] = "کالا",
            ["STUF_FSK"] = "موجودی کالا",
            ["STUF_STK"] = "موجودی انبار",
            ["CUST_HESAB"] = "حساب مشتری",
            ["payorder"] = "درخواست پرداخت",
            ["SALA_DTL"] = "کاربر",
            ["SAL_CHEK"] = "دسترسی کاربر",
            ["PRICE_ELAMIE"] = "لیست قیمت",
            ["TAKHFIF_DEF"] = "تخفیف",
            ["TAKHPERS"] = "تخفیف مشتری",
        };

        private static readonly Dictionary<int, string> TagLabels = new()
        {
            [1] = "فاکتور خرید",
            [2] = "فاکتور فروش",
            [3] = "برگشت خرید",
            [4] = "برگشت فروش",
            [12] = "درخواست خرید",
            [20] = "پیش‌فاکتور",
            [24] = "سایر رسید انبار",
            [26] = "سایر حواله انبار",
        };

        private sealed class Statement
        {
            public int Start;
            public string Action = string.Empty;
            public string Table = string.Empty;
        }

        /// <summary>
        /// آیا این دستور احتمالاً نوشتنی است؟ فقط یک غربال ارزان با چند
        /// IndexOf؛ تشخیص دقیق در <see cref="Observe"/> انجام می‌شود.
        ///
        /// مسیرهای تراکنشی از این استفاده می‌کنند تا صفِ «تا زمان commit»
        /// با SELECT پر نشود. بدون این غربال، یک تراکنش که چند ده SELECT
        /// می‌زند سقف صف را پر می‌کرد و نوشتن‌های واقعی از سابقه می‌افتادند.
        /// </summary>
        public static bool LooksLikeWrite(string? sql)
        {
            if (string.IsNullOrEmpty(sql)) return false;
            return sql.IndexOf("INSERT", StringComparison.OrdinalIgnoreCase) >= 0
                || sql.IndexOf("UPDATE", StringComparison.OrdinalIgnoreCase) >= 0
                || sql.IndexOf("DELETE", StringComparison.OrdinalIgnoreCase) >= 0
                || sql.IndexOf("EXEC", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>
        /// بررسی یک دستور SQL و در صورت نوشتنی بودن، ثبت رویداد.
        /// هرگز استثنا پرتاب نمی‌کند.
        /// </summary>
        /// <param name="parameters">شیء پارامترهای Dapper، برای استخراج مقدار واقعی کلید رکورد.</param>
        public static void Observe(string? sql, object? parameters = null, string? formName = null)
        {
            if (!Enabled || !AuditService.IsRunning) return;
            if (string.IsNullOrEmpty(sql)) return;

            try
            {
                // خروج سریع: اکثر قریب‌به‌اتفاق دستورها SELECT هستند.
                var hasInsert = sql.IndexOf("INSERT", StringComparison.OrdinalIgnoreCase) >= 0;
                var hasUpdate = sql.IndexOf("UPDATE", StringComparison.OrdinalIgnoreCase) >= 0;
                var hasDelete = sql.IndexOf("DELETE", StringComparison.OrdinalIgnoreCase) >= 0;

                if (!hasInsert && !hasUpdate && !hasDelete)
                {
                    // رویه‌ی ذخیره‌شده: نوشتن داخل رویه از دید این لایه پنهان
                    // است، ولی خودِ «اجرای رویه» رویداد مهمی است — مثل
                    // SP_PAY2_FINALIZE_RUN که لیست حقوق را نهایی می‌کند.
                    ObserveProcedure(sql, formName);
                    return;
                }

                // همه‌ی statementهای نوشتنی، نه فقط اولی. یک دستور واحد ممکن
                // است هم UPDATE هدر و هم چند INSERT ردیف داشته باشد؛ نسخه‌ی
                // قبلی فقط اولی را می‌دید و بقیه بی‌صدا گم می‌شدند.
                var statements = new List<Statement>();
                if (hasUpdate) Collect(statements, RxUpdate, sql, AuditAction.Update);
                if (hasInsert) Collect(statements, RxInsert, sql, AuditAction.Insert);
                if (hasDelete)
                {
                    Collect(statements, RxDeleteFrom, sql, AuditAction.Delete);
                    if (statements.Count == 0) Collect(statements, RxDeleteBare, sql, AuditAction.Delete);
                }

                if (statements.Count == 0) return;

                statements.Sort((a, b) => a.Start.CompareTo(b.Start));

                var count = Math.Min(statements.Count, MaxStatementsPerCommand);
                for (var i = 0; i < count; i++)
                {
                    var st = statements[i];
                    if (Excluded.Contains(st.Table)) continue;

                    // دامنه‌ی هر statement تا شروع statement بعدی است، وگرنه
                    // کلیدِ statement بعدی به این یکی نسبت داده می‌شود.
                    var end = (i + 1 < statements.Count) ? statements[i + 1].Start : sql.Length;
                    var segment = sql.Substring(st.Start, end - st.Start);

                    Emit(st, segment, parameters, formName);
                }
            }
            catch (Exception)
            {
                // تشخیص سابقه تحت هیچ شرایطی نباید دستور اصلی کاربر را خراب کند.
            }
        }

        private static readonly Regex RxExec =
            new(@"\bEXEC(?:UTE)?\s+(?:\[?dbo\]?\s*\.\s*)?\[?([A-Za-z_][A-Za-z0-9_]*)\]?", Opts);

        /// <summary>
        /// رویه‌های ذخیره‌شده‌ای که خودشان داده می‌نویسند. آنچه داخل رویه رخ
        /// می‌دهد از این لایه دیده نمی‌شود، ولی «چه کسی چه رویه‌ای را کِی
        /// اجرا کرد» خودش برای بررسی حیاتی است.
        ///
        /// رویه‌های صرفاً گزارشی عمداً اینجا نیستند تا سابقه شلوغ نشود.
        /// </summary>
        private static readonly HashSet<string> WritingProcedures = new(StringComparer.OrdinalIgnoreCase)
        {
            "SP_PAY2_FINALIZE_RUN",
            "SP_PAY2_FINALIZE_SETTLE",
            "SP_PAY2_CLOSE_PERIOD",
            "SP_PAY2_REVERT_RUN",
            "SP_SYS_AUDIT_PURGE",
            "SP_SYS_AUDIT_BACKFILL",
        };

        private static void ObserveProcedure(string sql, string? formName)
        {
            var m = RxExec.Match(sql);
            if (!m.Success) return;

            var name = m.Groups[1].Value;
            if (!WritingProcedures.Contains(name)) return;

            Audit.Security(AuditAction.ExecProcedure, $"اجرای رویه‌ی {name}", entity: name, formName: formName);
        }

        private static void Collect(List<Statement> into, Regex rx, string sql, string action)
        {
            foreach (Match match in rx.Matches(sql))
            {
                if (!match.Success) continue;
                var table = match.Groups[1].Value;
                if (string.IsNullOrEmpty(table)) continue;
                into.Add(new Statement { Start = match.Index, Action = action, Table = table });
                if (into.Count >= MaxStatementsPerCommand * 2) break;
            }
        }

        private static void Emit(Statement st, string segment, object? parameters, string? formName)
        {
            // کلید رکورد فقط از بخش WHERE همین statement خوانده می‌شود. اگر
            // کل دستور جستجو شود، در «UPDATE X SET CODE='A' WHERE ID=5» مقدار
            // جدیدِ CODE به‌جای شناسه‌ی رکورد برداشته می‌شود.
            var whereAt = segment.LastIndexOf("WHERE", StringComparison.OrdinalIgnoreCase);
            var scope = whereAt >= 0 ? segment.Substring(whereAt) : segment;

            int? tag = null;
            var tagMatch = RxTag.Match(scope);
            if (tagMatch.Success && int.TryParse(tagMatch.Groups[1].Value, out var t)) tag = t;

            string? key = null;
            var keyMatch = RxKey.Match(scope);
            if (keyMatch.Success)
            {
                var raw = keyMatch.Groups[2].Value;
                var value = NormalizeValue(raw, parameters);
                if (value != null)
                {
                    key = keyMatch.Groups[1].Value + "=" + value;
                    if (tag.HasValue) key += ";TAG=" + tag.Value;
                }
            }
            if (key is null && tag.HasValue) key = "TAG=" + tag.Value;

            var label = DescribeTable(st.Table, tag);
            var suffix = key is null ? string.Empty : " " + key;

            Audit.Data(st.Action, st.Table, key,
                       persianTitle: VerbOf(st.Action) + " " + label + suffix,
                       formName: formName,
                       oldValue: null, newValue: null);

            // امضا: یک UPDATE امضا معمولاً همه‌ی خانه‌ها را با هم می‌نویسد
            // (SGN1=0, SGN2=0, SGN3=1). گرفتن فقط اولین تطبیق — کاری که
            // نسخه‌ی قبلی می‌کرد — خانه و جهت را اشتباه گزارش می‌کرد. پس
            // وضعیت کاملِ همه‌ی خانه‌ها ثبت می‌شود و تغییر واقعی از مقایسه‌ی
            // دو رویداد پشت‌سرهم به‌دست می‌آید.
            if (st.Action == AuditAction.Update)
            {
                EmitSignState(st, segment, key, label, formName);
            }
        }

        private static void EmitSignState(Statement st, string segment, string? key, string label, string? formName)
        {
            var slots = new SortedDictionary<int, bool>();
            foreach (Match sign in RxSign.Matches(segment))
            {
                if (int.TryParse(sign.Groups[1].Value, out var slot)
                    && int.TryParse(sign.Groups[2].Value, out var value))
                {
                    slots[slot] = value != 0;
                }
            }
            if (slots.Count == 0) return;

            var parts = new List<string>(slots.Count);
            var anySigned = false;
            foreach (var kv in slots)
            {
                parts.Add($"{kv.Key}:{(kv.Value ? "بله" : "خیر")}");
                if (kv.Value) anySigned = true;
            }

            var suffix = key is null ? string.Empty : " " + key;
            Audit.SignState(st.Table, key, anySigned,
                            $"وضعیت امضای {label}{suffix} — {string.Join("، ", parts)}",
                            slots, formName);
        }

        /// <summary>
        /// تبدیل مقدار خام داخل SQL به مقدار قابل ذخیره.
        /// اگر پارامتر Dapper باشد (@CODE) مقدار واقعی از شیء پارامترها خوانده
        /// می‌شود؛ اگر قابل استخراج نبود null برمی‌گرداند تا به‌جای کلید،
        /// چیز بی‌معنی ذخیره نشود.
        /// </summary>
        private static string? NormalizeValue(string raw, object? parameters)
        {
            if (string.IsNullOrEmpty(raw)) return null;

            if (raw[0] == '@')
                return ResolveParameter(parameters, raw.Substring(1));

            var value = raw;
            if (value.Length > 1 && (value[0] == 'N' || value[0] == 'n') && value[1] == '\'')
                value = value.Substring(1);
            value = value.Trim('\'');

            value = value.Trim();
            return value.Length == 0 || value.Length > 60 ? null : value;
        }

        private static readonly ConcurrentDictionary<string, PropertyInfo?> _propCache = new();

        private static string? ResolveParameter(object? parameters, string name)
        {
            if (parameters is null || string.IsNullOrEmpty(name)) return null;

            try
            {
                if (parameters is DynamicParameters dyn)
                {
                    // بررسی وجود پیش از Get: متد Get وقتی پارامتر نباشد استثنا
                    // پرتاب می‌کند و پرتاب استثنا در مسیر داغِ هر نوشتن،
                    // هزینه‌ی واقعی دارد — حتی وقتی گرفته می‌شود.
                    if (!dyn.ParameterNames.Contains(name, StringComparer.OrdinalIgnoreCase)) return null;
                    var v = dyn.Get<object?>(name);
                    return v?.ToString();
                }

                var type = parameters.GetType();
                var prop = _propCache.GetOrAdd(
                    type.FullName + "|" + name,
                    _ => type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase));

                return prop?.GetValue(parameters)?.ToString();
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static string DescribeTable(string table, int? tag)
        {
            if (tag.HasValue && TagLabels.TryGetValue(tag.Value, out var tagLabel)
                && table.Equals("HEAD_LST", StringComparison.OrdinalIgnoreCase))
            {
                return tagLabel;
            }
            return TableLabels.TryGetValue(table, out var label) ? label : table;
        }

        private static string VerbOf(string action) => action switch
        {
            AuditAction.Insert => "ثبت",
            AuditAction.Update => "ویرایش",
            AuditAction.Delete => "حذف",
            _ => action,
        };
    }
}
