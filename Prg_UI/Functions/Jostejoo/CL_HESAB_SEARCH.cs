using Prg_UI.Wins.WinOther;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Media;

namespace Prg_UI.Functions.Jostejoo
{
    public static class CL_HESAB_SEARCH
    {
        // ورودی شبیه کد حساب: حداقل دو بخش عددی جدا شده با خط‌تیره (مثلا 101-2 یا 101-2-3 یا 101-2-3-4).
        // عمداً عدد تنها (بدون خط‌تیره) را شامل نمی‌شود؛ چون این تابع برای جستجوی بر اساس نام هم به کار
        // می‌رود و کاربر ممکن است یک عدد ساده را به‌عنوان بخشی از نام تایپ کند — آن حالت باید همچنان با
        // LIKE روی نام جستجو شود تا رفتار قبلی این فرم‌ها حفظ شود.
        private static readonly Regex AccountCodePattern = new Regex(@"^\d{1,9}(-\d{1,9}){1,3}$", RegexOptions.Compiled);

        public static void Go_Search_Hesab(string _TXT_, string FRMNAME, Visual thevisu)
        {
            var rawTerm = (_TXT_ ?? string.Empty).Trim();

            string shart = null;
            if (!string.IsNullOrEmpty(rawTerm))
            {
                shart = BuildAccountCodeWhere(rawTerm) ?? BuildNameSearchWhere(rawTerm);
            }

            var resOfSearch = new ResOfSearch(shart, FRMNAME, null, thevisu);
            resOfSearch.ShowDialog();
        }

        // اگر ورودی شبیه کد حساب باشد (مثل 101-2-3)، با تطبیق دقیق (=) روی ستون‌های کد جستجو می‌کنیم.
        // این کار برخلاف LIKE '%...%' روی ایندکس قابل استفاده است (Sargable) و خیلی سریع‌تر اجرا می‌شود؛
        // دقیقا همین حالت پرتکرار، یعنی تایپ کد حساب هنگام ورود سریع ردیف‌های سند، کند بودن جستجو را محسوس می‌کرد.
        private static string BuildAccountCodeWhere(string rawTerm)
        {
            var normalized = NormalizeAccountCode(rawTerm);
            if (!AccountCodePattern.IsMatch(normalized))
                return null;

            var parts = normalized.Split('-');
            var numbers = new int[parts.Length];
            for (int i = 0; i < parts.Length; i++)
            {
                if (!int.TryParse(parts[i], out numbers[i]))
                    return null;
            }

            var conditions = new List<string> { $"N_KOL = {numbers[0]}" };
            if (numbers.Length >= 2) conditions.Add($"NUMBER = {numbers[1]}");
            if (numbers.Length >= 3) conditions.Add($"TNUMBER = {numbers[2]}");
            if (numbers.Length == 4) conditions.Add($"tnumber2 = N'{string.Join("-", numbers)}'");

            return string.Join(" AND ", conditions);
        }

        // جستجوی نام: متن به کلمات تقسیم می‌شود و هر کلمه باید (در یکی از حالت‌های نوشتاری ی/ي و ک/ك)
        // در NAME یا در TNAME وجود داشته باشد. مقادیر همیشه Escape می‌شوند تا نه کوئری بشکند و نه
        // کاراکترهای خاص LIKE (% _ [ ]) به عنوان الگو تفسیر شوند.
        private static string BuildNameSearchWhere(string rawTerm)
        {
            var words = rawTerm.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                               .Select(w => w.Trim())
                               .Where(w => w.Length > 0)
                               .ToList();

            if (words.Count == 0)
                return null;

            var nameClauses = new List<string>();
            var tnameClauses = new List<string>();

            foreach (var word in words)
            {
                var variants = new[]
                {
                    EscapeForLike(word),
                    EscapeForLike(ToArabicVariant(word)),
                    EscapeForLike(ToPersianVariant(word))
                }.Distinct().ToList();

                nameClauses.Add("(" + string.Join(" OR ", variants.Select(v => $"NAME LIKE N'%{v}%'")) + ")");
                tnameClauses.Add("(" + string.Join(" OR ", variants.Select(v => $"TNAME LIKE N'%{v}%'")) + ")");
            }

            var byName = string.Join(" AND ", nameClauses);
            var byTName = string.Join(" AND ", tnameClauses);

            return $"(({byName}) OR ({byTName}))";
        }

        private static string ToArabicVariant(string value) => value.Replace('ی', 'ي').Replace('ک', 'ك');

        private static string ToPersianVariant(string value) => value.Replace('ي', 'ی').Replace('ك', 'ک');

        // جلوگیری از SQL Injection و از تفسیر شدن کاراکترهای خاص LIKE به عنوان الگو
        private static string EscapeForLike(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;

            return value
                .Replace("'", "''")
                .Replace("[", "[[]")
                .Replace("%", "[%]")
                .Replace("_", "[_]");
        }

        private static string NormalizeAccountCode(string value)
        {
            var sb = new StringBuilder(value.Length);
            foreach (char ch in value.Trim())
            {
                if (ch >= '0' && ch <= '9')
                    sb.Append(ch);
                else if (ch >= '۰' && ch <= '۹')
                    sb.Append((char)('0' + (ch - '۰')));
                else if (ch >= '٠' && ch <= '٩')
                    sb.Append((char)('0' + (ch - '٠')));
                else if (ch == '-' || ch == '‐' || ch == '–' || ch == '—' || ch == '−')
                    sb.Append('-');
                else if (!char.IsWhiteSpace(ch))
                    sb.Append(ch);
            }
            return sb.ToString();
        }
    }
}
