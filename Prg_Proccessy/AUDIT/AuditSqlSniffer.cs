using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

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
    /// پس تشخیص در همان سه متد مرکزی دسترسی به داده انجام می‌شود. چون این
    /// متدها متن نهایی دستور را می‌بینند (بعد از جای‌گذاری مقادیر)، مقادیری
    /// مثل TAG و شماره‌ی سند هم قابل استخراج‌اند.
    ///
    /// هزینه: برای دستورهای خواندنی سه IndexOf ساده و خروج فوری. برای
    /// دستورهای نوشتنی چند Regex از پیش کامپایل‌شده — در برابر رفت‌وبرگشت
    /// خودِ آن دستور به دیتابیس ناچیز است.
    /// </summary>
    public static class AuditSqlSniffer
    {
        /// <summary>شیر اصلی. اگر حجم سابقه مشکل‌ساز شد، با false کردن این، تشخیص خودکار خاموش می‌شود.</summary>
        public static bool Enabled { get; set; } = true;

        private static readonly RegexOptions Opts =
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;

        private static readonly Regex RxInsert =
            new(@"\bINSERT\s+INTO\s+(?:\[?dbo\]?\s*\.\s*)?\[?([A-Za-z_][A-Za-z0-9_]*)\]?", Opts);

        private static readonly Regex RxUpdate =
            new(@"\bUPDATE\s+(?:TOP\s*\([^)]*\)\s*)?(?:\[?dbo\]?\s*\.\s*)?\[?([A-Za-z_][A-Za-z0-9_]*)\]?\s+SET\b", Opts);

        // دو الگو برای DELETE: اول شکل FROM‌دار، چون «DELETE d FROM dbo.X»
        // با الگوی ساده نام مستعار (d) را به‌جای نام جدول می‌گیرد.
        private static readonly Regex RxDeleteFrom =
            new(@"\bDELETE\b[^;]{0,120}?\bFROM\s+(?:\[?dbo\]?\s*\.\s*)?\[?([A-Za-z_][A-Za-z0-9_]*)\]?", Opts);

        private static readonly Regex RxDelete =
            new(@"\bDELETE\s+(?:TOP\s*\([^)]*\)\s*)?(?:\[?dbo\]?\s*\.\s*)?\[?([A-Za-z_][A-Za-z0-9_]*)\]?", Opts);

        /// <summary>ستون‌های کلیدی رایج، به ترتیب اولویت.</summary>
        private static readonly Regex RxKey =
            new(@"\b(N_S|NUMBER1|NUMBER|IDH|IDD|CODE|N_SERI|ID)\s*=\s*N?'?([^'\s,)]+)'?", Opts);

        private static readonly Regex RxTag =
            new(@"\bTAG\s*=\s*(\d{1,3})\b", Opts);

        /// <summary>امضا: SGN1 = 1 یا SGN2 =0 و مانند آن.</summary>
        private static readonly Regex RxSign =
            new(@"\bSGN(\d)\s*=\s*(\d)", Opts);

        /// <summary>
        /// جدول‌هایی که ثبت نمی‌شوند. خودِ جدول‌های سابقه حتماً باید اینجا
        /// باشند تا حلقه‌ی بی‌پایان ایجاد نشود، و جدول‌های پرترافیکِ بی‌ارزش
        /// هم برای کنترل حجم کنار گذاشته شده‌اند.
        /// </summary>
        private static readonly HashSet<string> Excluded = new(StringComparer.OrdinalIgnoreCase)
        {
            "SYS_AUDIT_EVENT", "SYS_AUDIT_SESSION", "AMALIAT", "USER_AUDIT_LOG",
            "UserState", "PAY2_SEC_AUDIT", "CC_RUNLOG", "SMS_SENDS",
        };

        /// <summary>نام فارسی جدول‌ها، فقط برای خوانا شدن ستون شرح.</summary>
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
            ["CUST_HESAB"] = "حساب مشتری",
            ["payorder"] = "درخواست پرداخت",
            ["SALA_DTL"] = "کاربر",
            ["SAL_CHEK"] = "دسترسی کاربر",
            ["PRICE_ELAMIE"] = "لیست قیمت",
            ["TAKHFIF_DEF"] = "تخفیف",
            ["TAKHPERS"] = "تخفیف مشتری",
        };

        /// <summary>نام فارسی نوع برگه بر اساس TAG در HEAD_LST.</summary>
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

        /// <summary>
        /// بررسی یک دستور SQL و در صورت نوشتنی بودن، ثبت رویداد.
        /// هرگز استثنا پرتاب نمی‌کند و هرگز کار فراخوان را کند نمی‌کند.
        /// </summary>
        public static void Observe(string? sql, string? formName = null)
        {
            if (!Enabled || !AuditService.IsRunning) return;
            if (string.IsNullOrEmpty(sql)) return;

            try
            {
                // خروج سریع: اکثر قریب‌به‌اتفاق دستورها SELECT هستند.
                var hasInsert = sql.IndexOf("INSERT", StringComparison.OrdinalIgnoreCase) >= 0;
                var hasUpdate = sql.IndexOf("UPDATE", StringComparison.OrdinalIgnoreCase) >= 0;
                var hasDelete = sql.IndexOf("DELETE", StringComparison.OrdinalIgnoreCase) >= 0;
                if (!hasInsert && !hasUpdate && !hasDelete) return;

                string? action = null;
                Match? m = null;

                if (hasUpdate)
                {
                    var candidate = RxUpdate.Match(sql);
                    if (candidate.Success) { m = candidate; action = AuditAction.Update; }
                }
                if (m is null && hasInsert)
                {
                    var candidate = RxInsert.Match(sql);
                    if (candidate.Success) { m = candidate; action = AuditAction.Insert; }
                }
                if (m is null && hasDelete)
                {
                    // اول شکل FROM‌دار، بعد شکل ساده.
                    var candidate = RxDeleteFrom.Match(sql);
                    if (!candidate.Success) candidate = RxDelete.Match(sql);
                    if (candidate.Success) { m = candidate; action = AuditAction.Delete; }
                }

                if (m is null || action is null) return;

                var table = m.Groups[1].Value;
                if (string.IsNullOrEmpty(table) || Excluded.Contains(table)) return;

                // کلید رکورد فقط از بخش WHERE خوانده می‌شود. اگر کل دستور را
                // جستجو کنیم، در «UPDATE X SET CODE='A' WHERE ID=5» مقدار جدیدِ
                // CODE به‌جای شناسه‌ی رکورد برداشته می‌شود.
                var whereAt = sql.LastIndexOf("WHERE", StringComparison.OrdinalIgnoreCase);
                var scope = whereAt >= 0 ? sql.Substring(whereAt) : sql;

                int? tag = null;
                var tagMatch = RxTag.Match(scope);
                if (tagMatch.Success && int.TryParse(tagMatch.Groups[1].Value, out var t)) tag = t;

                string? key = null;
                var keyMatch = RxKey.Match(scope);
                if (keyMatch.Success)
                {
                    key = keyMatch.Groups[1].Value + "=" + keyMatch.Groups[2].Value;
                    if (tag.HasValue) key += ";TAG=" + tag.Value;
                }
                else if (tag.HasValue)
                {
                    key = "TAG=" + tag.Value;
                }

                var label = DescribeTable(table, tag);

                // امضا یک رویداد کاری است، نه صرفاً یک UPDATE.
                // این مسیر هر ۶۳ نقطه‌ی امضای نرم‌افزار را یکجا پوشش می‌دهد.
                if (action == AuditAction.Update)
                {
                    var sign = RxSign.Match(sql);
                    if (sign.Success
                        && int.TryParse(sign.Groups[1].Value, out var slot)
                        && int.TryParse(sign.Groups[2].Value, out var value))
                    {
                        Audit.Sign(table, key, slot, value != 0, formName,
                                   (value != 0 ? "امضای " : "برداشتن امضای ")
                                   + label + (key is null ? "" : " " + key)
                                   + $" (امضای {slot})");
                        return;
                    }
                }

                Audit.Data(action, table, key,
                           persianTitle: VerbOf(action) + " " + label + (key is null ? "" : " " + key),
                           formName: formName,
                           oldValue: null, newValue: null);
            }
            catch (Exception)
            {
                // تشخیص سابقه تحت هیچ شرایطی نباید دستور اصلی کاربر را خراب کند.
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
