using System.Linq;

namespace System
{
    public static class StringExtensions
    {
        const char arabicYa01 = '\u0649';
        const char arabicYa02 = '\u064A';
        const char arabicKaf = '\u0643';

        const char farsiYa = '\u06CC';
        const char farsiKaf = '\u06A9';

        public static string ArabicToFarsi(this string arabicString)
        {
            if (string.IsNullOrWhiteSpace(arabicString))
                return arabicString;

            return arabicString
                .Replace(arabicYa01, farsiYa)
                .Replace(arabicYa02, farsiYa)
                .Replace(arabicKaf, farsiKaf);
        }
        /// <summary>
        /// Removes various zero-width or invisible Unicode characters
        /// (and also does a normal Trim).
        /// </summary>
        public static string RemoveInvisibleChars(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // لیست کامل کاراکترهای نامرئی و کنترلی (قابل افزایش)
            char[] invisibleChars = new char[]
            {
                '\u200B', // Zero-width space
                '\u200C', // Zero-width non-joiner
                '\u200D', // Zero-width joiner
                '\u200E', // Left-to-right mark (LRM)
                '\u200F', // Right-to-left mark (RLM)
                '\u202A', // LRE
                '\u202B', // RLE
                '\u202C', // PDF
                '\u202D', // LRO
                '\u202E', // RLO
                '\u2060', // Word Joiner
                '\uFEFF', // BOM
            };

            // حذف کاراکترهای بالا:
            foreach (char c in invisibleChars)
            {
                input = input.Replace(c.ToString(), string.Empty);
            }

            // جایگزینی کاراکترهای فاصله نامتعارف با فاصله عادی:
            input = input
                .Replace('\t', ' ')
                .Replace('\n', ' ')
                .Replace('\r', ' ')
                .Replace('\f', ' ')
                .Replace('\v', ' ');

            // حذف هر کاراکتر کنترلی باقی‌مانده:
            input = new string(input.Where(ch => !char.IsControl(ch)).ToArray());

            // حذف فاصله‌های اضافه ابتدا و انتها:
            input = input.Trim();

            return input;
        }


        public static string FixPersianChars(this string str)
        {
            if (!string.IsNullOrEmpty(str) && !string.IsNullOrWhiteSpace(str))
            {
                //.Replace("ئ", "ی");
                return str.Replace("ﮎ", "ک")
                .Replace("ﮏ", "ک")
                .Replace("ﮐ", "ک")
                .Replace("ﮑ", "ک")
                .Replace("ك", "ک")
                .Replace("ي", "ی")
                .Replace("ھ", "ه")

                .Replace('\u064A', 'ی') // Arabic Yeh
                .Replace('\u0643', 'ک') // Arabic Kaf
                .Replace('\u06C0', 'ه') // Heh with Yeh Above
                .Replace('\u200C', ' ') // ZWNJ to space (یا حذفش کن)



                .Replace('۰', '0')
                .Replace('۱', '1')
                .Replace('۲', '2')
                .Replace('۳', '3')
                .Replace('۴', '4')
                .Replace('۵', '5')
                .Replace('۶', '6')
                .Replace('۷', '7')
                .Replace('۸', '8')
                .Replace('۹', '9');
            }
            return str;
        }
        public static string FixPersianDigits(this string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            // Replace Persian digits (۰١٢٣٤٥٦٧٨٩) with ASCII digits (0-9).
            return input
               .Replace('۰', '0')
               .Replace('۱', '1')
               .Replace('۲', '2')
               .Replace('۳', '3')
               .Replace('۴', '4')
               .Replace('۵', '5')
               .Replace('۶', '6')
               .Replace('۷', '7')
               .Replace('۸', '8')
               .Replace('۹', '9');
        }

        public static string NormalizeSpaces(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;
            return System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ").Trim();
        }

        public static string LatinNumbersToFarsiNumbers(this string value)
        {
            return value.Replace('1', '۱')
                    .Replace('2', '۲')
                    .Replace('3', '۳')
                    .Replace('4', '۴')
                    .Replace('5', '۵')
                    .Replace('6', '۶')
                    .Replace('7', '۷')
                    .Replace('8', '۸')
                    .Replace('9', '۹')
                    .Replace('0', '۰')
                    .Replace('.', '\u066B');
        }

        public static string FarsiNumbersToLatinNumbers(this string value)
        {
            return value.Replace('۱', '1')
                        .Replace('۲', '2')
                        .Replace('۳', '3')
                        .Replace('۴', '4')
                        .Replace('۵', '5')
                        .Replace('۶', '6')
                        .Replace('۷', '7')
                        .Replace('۸', '8')
                        .Replace('۹', '9')
                        .Replace('۰', '0')
                        .Replace('\u066B', '.')
                        //iphone numeric
                        .Replace("٠", "0")
                        .Replace("١", "1")
                        .Replace("٢", "2")
                        .Replace("٣", "3")
                        .Replace("٤", "4")
                        .Replace("٥", "5")
                        .Replace("٦", "6")
                        .Replace("٧", "7")
                        .Replace("٨", "8")
                        .Replace("٩", "9");
        }

        public static bool ContainsIgnoreCase(this string value, string term)
        {
            return value.IndexOf(term, StringComparison.InvariantCultureIgnoreCase) >= 0;
        }

        public static bool EqualsIgnoreCase(this string theString, string value)
        {
            return theString.Equals(value, StringComparison.InvariantCultureIgnoreCase);
        }

        public static string ToBase64String(this string value)
        {
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(value));
        }

        public static string Base64ToNormalString(this string base64String)
        {
            return System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(base64String));
        }


    }
}
