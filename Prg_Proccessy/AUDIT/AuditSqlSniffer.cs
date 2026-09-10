using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
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

        /// <summary>
        /// آیا مقادیر جدیدِ فیلدها در ستون DETAIL ذخیره شود؟
        ///
        /// مقدارِ «بعد» بدون هیچ هزینه‌ای در دسترس است، چون بخش SET همین
        /// دستوری است که همین الان اجرا شده. مقدارِ «قبل» در دسترس نیست و
        /// گرفتنش یک SELECT پیش از هر UPDATE می‌خواهد — که خلاف قید «نباید
        /// کندی ایجاد شود» است. برای مقدار قبل باید به جدول‌های TR_ مراجعه شود.
        ///
        /// اگر سیاست سازمان اجازه‌ی نگه‌داری مقادیر کسب‌وکار در سابقه را
        /// نمی‌دهد، با false کردن این، فقط نام ستون‌های تغییرکرده ثبت می‌شود.
        /// </summary>
        public static bool CaptureNewValues { get; set; } = true;

        /// <summary>
        /// ستون‌هایی که مقدارشان هرگز ذخیره نمی‌شود. نام ستون ثبت می‌شود ولی
        /// مقدار با *** جایگزین می‌گردد. PSAL_NAME ستون رمز عبور کاربران است.
        /// </summary>
        private static readonly HashSet<string> RedactedColumns = new(StringComparer.OrdinalIgnoreCase)
        {
            "PSAL_NAME", "PASSWORD", "PASSWD", "PASS", "RMZ", "RAMZ",
            "TOKEN", "APIKEY", "API_KEY", "SECRET", "SERIAL", "LICENSE",
        };

        private const int MaxCapturedColumns = 25;
        private const int MaxCapturedValueLength = 60;

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
                    //
                    // شرط IndexOf لازم است: بدون آن، هر SELECT ساده هم یک
                    // Regex اضافه اجرا می‌کرد و این متد روی مسیر تمام خواندن‌های
                    // نرم‌افزار قرار دارد.
                    if (sql.IndexOf("EXEC", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        ObserveProcedure(sql, formName);
                    }
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

                    // دامنه‌ی هر statement تا شروع statement بعدی است، وگرنه
                    // کلیدِ statement بعدی به این یکی نسبت داده می‌شود.
                    var end = (i + 1 < statements.Count) ? statements[i + 1].Start : sql.Length;
                    var segment = sql.Substring(st.Start, end - st.Start);

                    // در شکل «UPDATE vd SET ... FROM ... JOIN dbo.VISITOR_DTL vd»
                    // آنچه بعد از UPDATE آمده نام مستعار است. باید پیش از
                    // فیلتر Excluded باز شود، وگرنه جدولی که باید نادیده
                    // گرفته شود از زیر نام مستعارش رد می‌شود.
                    if (st.Action == AuditAction.Update)
                        st.Table = ResolveUpdateAlias(segment, st.Table);

                    if (Excluded.Contains(st.Table)) continue;

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
            "SYS_AUDIT_PURGE",
            "SYS_AUDIT_BACKFILL",
        };

        private static void ObserveProcedure(string sql, string? formName)
        {
            var m = RxExec.Match(sql);
            if (!m.Success) return;

            var name = m.Groups[1].Value;
            if (!WritingProcedures.Contains(name)) return;

            Audit.Security(AuditAction.ExecProcedure, $"اجرای رویه‌ی {name}", entity: name, formName: formName);
        }

        /// <summary>واژه‌های کلیدی بند FROM که جدول نیستند.</summary>
        private static readonly HashSet<string> JoinNoise = new(StringComparer.OrdinalIgnoreCase)
        {
            "INNER", "LEFT", "RIGHT", "FULL", "OUTER", "CROSS", "JOIN",
            "APPLY", "ON", "AS", "WITH", "NOLOCK",
        };

        private static readonly Regex RxFromBinding =
            new(@"(?:\[?dbo\]?\s*\.\s*)?\[?([A-Za-z_][A-Za-z0-9_]*)\]?\s+(?:AS\s+)?\[?([A-Za-z_][A-Za-z0-9_]*)\]?", Opts);

        /// <summary>
        /// در «UPDATE vd SET ... FROM dbo.HEAD_LST hl INNER JOIN dbo.VISITOR_DTL vd ON ...»
        /// آنچه بین UPDATE و SET می‌آید نام مستعار است، نه جدول. اگر بند FROM
        /// آن نام را به جدولی ببندد، نام واقعی جدول برگردانده می‌شود.
        ///
        /// بدون این، رویداد زیر نام «vd» ثبت می‌شد و جست‌وجو بر اساس نام
        /// جدول پیدایش نمی‌کرد. دو مورد واقعی در همین کدبیس:
        /// ZASESABBEESAB.xaml.cs خطوط ۵۹۸ و ۶۰۹. برای DELETE این حالت از
        /// قبل با RxDeleteFrom پوشش داده شده بود، برای UPDATE نه.
        /// </summary>
        private static string ResolveUpdateAlias(string segment, string name)
        {
            var fromAt = IndexOfKeyword(segment, "FROM");
            if (fromAt < 0) return name;

            var from = segment.Substring(fromAt + 4);
            var stopAt = IndexOfKeyword(from, "WHERE");
            if (stopAt >= 0) from = from.Substring(0, stopAt);

            foreach (Match m in RxFromBinding.Matches(from))
            {
                var table = m.Groups[1].Value;
                var alias = m.Groups[2].Value;
                if (JoinNoise.Contains(table) || JoinNoise.Contains(alias)) continue;
                if (alias.Equals(name, StringComparison.OrdinalIgnoreCase)) return table;
            }

            return name;
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
            // LastIndexOf آخرین WHERE را برمی‌دارد، و در
            // «UPDATE X SET A=1 WHERE ID IN (SELECT ... WHERE Y=2)» آن WHERE
            // متعلق به زیرکوئری است — پس کلید رکورد از جدول اشتباه خوانده
            // می‌شد. IndexOfKeyword پرانتز و رشته را می‌فهمد و اولین WHERE
            // سطح بالا را می‌دهد، که همان WHERE خودِ statement است.
            var whereAt = IndexOfKeyword(segment, "WHERE");
            var scope = whereAt >= 0 ? segment.Substring(whereAt) : segment;

            int? tag = null;
            var tagMatch = RxTag.Match(scope);
            if (tagMatch.Success && int.TryParse(tagMatch.Groups[1].Value, out var t)) tag = t;

            string? key = null;

            if (st.Action == AuditAction.Insert)
            {
                // در INSERT کلید داخل WHERE نیست، در VALUES است و جایگاهش با
                // فهرست ستون‌ها مشخص می‌شود. بدون این، رویدادِ «ایجاد سند»
                // شماره‌ی سند را نداشت و جستجوی چرخه‌ی عمر یک فاکتور،
                // لحظه‌ی ساخته شدنش را نشان نمی‌داد.
                key = BuildInsertKey(segment, parameters, ref tag);
            }
            else
            {
                var keyMatch = RxKey.Match(scope);
                if (keyMatch.Success)
                {
                    var value = NormalizeValue(keyMatch.Groups[2].Value, parameters);
                    if (value != null)
                    {
                        key = keyMatch.Groups[1].Value + "=" + value;
                        if (tag.HasValue) key += ";TAG=" + tag.Value;
                    }
                }
            }

            if (key is null && tag.HasValue) key = "TAG=" + tag.Value;

            var label = DescribeTable(st.Table, tag);
            var suffix = key is null ? string.Empty : " " + key;

            // برای UPDATE، ستون‌های تغییرکرده و مقدار جدیدشان از بخش SET
            // همین دستور خوانده می‌شود — بدون هیچ رفت‌وبرگشت اضافه‌ای.
            var detail = st.Action == AuditAction.Update
                ? BuildChangedColumns(segment, parameters)
                : null;

            Audit.DataWithDetail(st.Action, st.Table, key,
                                 persianTitle: VerbOf(st.Action) + " " + label + suffix,
                                 formName: formName,
                                 detail: detail);

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
                // شنونده‌ی مرکزی پارامترهای SqlCommand را به‌صورت دیکشنری
                // کپی می‌کند (نگه داشتن خودِ SqlParameterCollection پس از
                // پایان دستور امن نیست).
                if (parameters is IDictionary<string, object?> map)
                {
                    return map.TryGetValue(name, out var mv) ? mv?.ToString() : null;
                }

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

        /// <summary>ستون‌های کلیدی به ترتیب اولویت، برای تشخیص کلید در INSERT.</summary>
        private static readonly string[] KeyColumnPriority =
            { "N_S", "NUMBER", "NUMBER1", "IDH", "IDD", "CODE", "ID" };

        /// <summary>
        /// استخراج کلید رکورد از یک <c>INSERT ... (ستون‌ها) VALUES (مقادیر)</c>.
        ///
        /// ستون و مقدار با جایگاه به هم نگاشت می‌شوند. اگر دستور از نوع
        /// <c>INSERT ... SELECT</c> باشد یا فهرست ستون نداشته باشد، کلیدی
        /// قابل استخراج نیست و null برمی‌گردد.
        /// </summary>
        private static string? BuildInsertKey(string segment, object? parameters, ref int? tag)
        {
            try
            {
                // فهرست ستون‌ها: اولین پرانتز بعد از نام جدول.
                var open = segment.IndexOf('(');
                if (open < 0) return null;

                var close = MatchingParen(segment, open);
                if (close < 0) return null;

                var columns = SplitTopLevel(segment.Substring(open + 1, close - open - 1), ',');
                if (columns.Count == 0) return null;

                // VALUES بعد از فهرست ستون‌ها (ممکن است OUTPUT بینشان باشد).
                var rest = segment.Substring(close + 1);
                var valuesAt = IndexOfKeyword(rest, "VALUES");
                if (valuesAt < 0) return null;   // INSERT ... SELECT

                var vOpen = rest.IndexOf('(', valuesAt);
                if (vOpen < 0) return null;

                var vClose = MatchingParen(rest, vOpen);
                if (vClose < 0) return null;

                var values = SplitTopLevel(rest.Substring(vOpen + 1, vClose - vOpen - 1), ',');
                if (values.Count != columns.Count) return null;   // نگاشت جایگاهی مطمئن نیست

                string? Lookup(string wanted)
                {
                    for (var i = 0; i < columns.Count; i++)
                    {
                        var col = columns[i].Trim().Trim('[', ']', ' ');
                        if (!col.Equals(wanted, StringComparison.OrdinalIgnoreCase)) continue;
                        return NormalizeValue(values[i].Trim(), parameters);
                    }
                    return null;
                }

                if (!tag.HasValue)
                {
                    var t = Lookup("TAG");
                    if (t != null && int.TryParse(t, out var parsed)) tag = parsed;
                }

                foreach (var candidate in KeyColumnPriority)
                {
                    var v = Lookup(candidate);
                    if (string.IsNullOrEmpty(v)) continue;
                    return tag.HasValue ? $"{candidate}={v};TAG={tag.Value}" : $"{candidate}={v}";
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>جای پرانتز بسته‌ی متناظر، با در نظر گرفتن رشته‌ها.</summary>
        private static int MatchingParen(string text, int openIndex)
        {
            var depth = 0;
            var inQuote = false;
            for (var i = openIndex; i < text.Length; i++)
            {
                var c = text[i];
                if (inQuote) { if (c == '\'') inQuote = false; continue; }
                if (c == '\'') { inQuote = true; continue; }
                if (c == '(') depth++;
                else if (c == ')')
                {
                    depth--;
                    if (depth == 0) return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// استخراج ستون‌های تغییرکرده و مقدار جدیدشان از بخش SET یک UPDATE.
        ///
        /// عمداً با یک پیمایش دستی انجام می‌شود نه Regex: کاما و علامت مساوی
        /// می‌توانند داخل پرانتز (مثل <c>ISNULL(a, b)</c>) یا داخل رشته
        /// (<c>N'a, b'</c>) باشند و تقسیم ساده آن‌ها را خراب می‌کند.
        /// </summary>
        private static string? BuildChangedColumns(string segment, object? parameters)
        {
            // بخش بین SET و WHERE. اگر WHERE نباشد تا انتها.
            var setAt = IndexOfKeyword(segment, "SET");
            if (setAt < 0) return null;

            var body = segment.Substring(setAt + 3);
            var whereAt = IndexOfKeyword(body, "WHERE");
            if (whereAt >= 0) body = body.Substring(0, whereAt);

            var pairs = new List<KeyValuePair<string, string>>();

            foreach (var part in SplitTopLevel(body, ','))
            {
                if (pairs.Count >= MaxCapturedColumns) break;

                var eq = IndexOfTopLevel(part, '=');
                if (eq <= 0) continue;

                var column = part.Substring(0, eq).Trim().Trim('[', ']', ' ');
                if (column.Length == 0 || column.Length > 40) continue;
                // نام ستون باید شناسه باشد، نه عبارت.
                if (!IsIdentifier(column)) continue;

                if (!CaptureNewValues)
                {
                    pairs.Add(new KeyValuePair<string, string>(column, "?"));
                    continue;
                }

                if (RedactedColumns.Contains(column))
                {
                    pairs.Add(new KeyValuePair<string, string>(column, "***"));
                    continue;
                }

                var raw = part.Substring(eq + 1).Trim();

                // زیرکوئری یا عبارت طولانی ذخیره نمی‌شود؛ نه خوانا است نه امن.
                if (raw.Length == 0 || raw.Length > 200) continue;
                if (raw.IndexOf("SELECT", StringComparison.OrdinalIgnoreCase) >= 0) continue;

                var value = NormalizeValue(raw, parameters);
                if (value is null) continue;

                if (value.Length > MaxCapturedValueLength)
                    value = value.Substring(0, MaxCapturedValueLength) + "…";

                pairs.Add(new KeyValuePair<string, string>(column, value));
            }

            if (pairs.Count == 0) return null;

            try
            {
                var map = new Dictionary<string, string>(pairs.Count, StringComparer.OrdinalIgnoreCase);
                foreach (var kv in pairs) map[kv.Key] = kv.Value;
                return JsonSerializer.Serialize(new { changed = map }, Audit.JsonOptions);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static bool IsIdentifier(string s)
        {
            if (s.Length == 0) return false;
            if (!char.IsLetter(s[0]) && s[0] != '_') return false;
            for (var i = 1; i < s.Length; i++)
            {
                if (!char.IsLetterOrDigit(s[i]) && s[i] != '_') return false;
            }
            return true;
        }

        /// <summary>جای یک کلیدواژه، بیرون از رشته و پرانتز.</summary>
        private static int IndexOfKeyword(string text, string keyword)
        {
            var depth = 0;
            var inQuote = false;

            for (var i = 0; i < text.Length; i++)
            {
                var c = text[i];
                if (inQuote) { if (c == '\'') inQuote = false; continue; }
                if (c == '\'') { inQuote = true; continue; }
                if (c == '(') { depth++; continue; }
                if (c == ')') { if (depth > 0) depth--; continue; }
                if (depth != 0) continue;

                if (i + keyword.Length > text.Length) continue;
                if (string.Compare(text, i, keyword, 0, keyword.Length, StringComparison.OrdinalIgnoreCase) != 0) continue;

                var before = i == 0 || !char.IsLetterOrDigit(text[i - 1]) && text[i - 1] != '_';
                var afterIdx = i + keyword.Length;
                var after = afterIdx >= text.Length || (!char.IsLetterOrDigit(text[afterIdx]) && text[afterIdx] != '_');
                if (before && after) return i;
            }
            return -1;
        }

        private static int IndexOfTopLevel(string text, char target)
        {
            var depth = 0;
            var inQuote = false;
            for (var i = 0; i < text.Length; i++)
            {
                var c = text[i];
                if (inQuote) { if (c == '\'') inQuote = false; continue; }
                if (c == '\'') { inQuote = true; continue; }
                if (c == '(') { depth++; continue; }
                if (c == ')') { if (depth > 0) depth--; continue; }
                if (depth == 0 && c == target) return i;
            }
            return -1;
        }

        private static List<string> SplitTopLevel(string text, char separator)
        {
            var parts = new List<string>();
            var depth = 0;
            var inQuote = false;
            var start = 0;

            for (var i = 0; i < text.Length; i++)
            {
                var c = text[i];
                if (inQuote) { if (c == '\'') inQuote = false; continue; }
                if (c == '\'') { inQuote = true; continue; }
                if (c == '(') { depth++; continue; }
                if (c == ')') { if (depth > 0) depth--; continue; }

                if (depth == 0 && c == separator)
                {
                    parts.Add(text.Substring(start, i - start));
                    start = i + 1;
                    if (parts.Count > MaxCapturedColumns * 2) return parts;
                }
            }

            if (start < text.Length) parts.Add(text.Substring(start));
            return parts;
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
