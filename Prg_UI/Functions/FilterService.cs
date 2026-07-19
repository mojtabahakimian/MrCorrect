using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Functions
{
    public class FilterService<T>
    {
        private readonly List<(string ColumnName, object FilterValue, bool IsExclusion, bool IsExactMatch, bool NormalizePersianText)> cumulativeFilters =
            new List<(string ColumnName, object FilterValue, bool IsExclusion, bool IsExactMatch, bool NormalizePersianText)>();

        /// <summary>
        /// اضافه کردن فیلتر جدید به لیست فیلترهای فعال
        /// </summary>
        /// <param name="columnName">نام ستون</param>
        /// <param name="filterValue">مقدار فیلتر (می‌تواند string، numeric، یا null باشد)</param>
        /// <param name="isExclusion">آیا فیلتر exclusion است؟</param>
        /// <param name="isExactMatch">آیا باید دقیقاً برابر باشد؟ (در غیر این صورت Contains)</param>
        /// <param name="normalizePersianText">آیا تفاوت نویسه‌های فارسی و عربی و فاصله‌ها نادیده گرفته شود؟</param>
        public void AddFilter(string columnName, object filterValue, bool isExclusion = false, bool isExactMatch = false, bool normalizePersianText = false)
        {
            cumulativeFilters.Add((columnName, filterValue, isExclusion, isExactMatch, normalizePersianText));
        }

        /// <summary>
        /// پاک کردن تمام فیلترها
        /// </summary>
        public void ClearFilters()
        {
            cumulativeFilters.Clear();
        }

        /// <summary>
        /// اعمال تمام فیلترها به یک آیتم
        /// </summary>
        public bool ApplyFilter(T item)
        {
            if (item == null) return false;

            foreach (var (columnName, filterValue, isExclusion, isExactMatch, normalizePersianText) in cumulativeFilters)
            {
                // دریافت مقدار واقعی property (نه ToString آن)
                var actualValue = GetPropValue(item, columnName);

                // اعمال فیلتر
                bool matchResult = EvaluateFilter(actualValue, filterValue, isExactMatch, normalizePersianText);

                // اگر exclusion است، نتیجه را برعکس می‌کنیم
                if (isExclusion)
                {
                    // اگر match شد، باید این آیتم را exclude کنیم
                    if (matchResult)
                        return false;
                }
                else
                {
                    // اگر match نشد، این آیتم را رد می‌کنیم
                    if (!matchResult)
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// ارزیابی یک فیلتر واحد
        /// </summary>
        private bool EvaluateFilter(object actualValue, object filterValue, bool isExactMatch, bool normalizePersianText)
        {
            // Handle null cases
            if (filterValue == null)
            {
                return actualValue == null;
            }

            if (actualValue == null)
            {
                return false;
            }

            // اگر filterValue یک string است
            if (filterValue is string filterString)
            {
                filterString = filterString.Trim();

                // چک کردن blank values
                if (string.IsNullOrWhiteSpace(filterString))
                {
                    return string.IsNullOrWhiteSpace(actualValue.ToString());
                }

                // تبدیل actualValue به string برای مقایسه متنی
                string actualString = actualValue.ToString()?.Trim() ?? string.Empty;

                if (normalizePersianText)
                {
                    filterString = NormalizePersianSearchText(filterString);
                    actualString = NormalizePersianSearchText(actualString);
                }

                if (isExactMatch)
                {
                    // مقایسه دقیق (case-insensitive)
                    return string.Equals(actualString, filterString, StringComparison.OrdinalIgnoreCase);
                }
                else
                {
                    // مقایسه Contains (case-insensitive)
                    return actualString.Contains(filterString, StringComparison.OrdinalIgnoreCase);
                }
            }

            // اگر هر دو عددی هستند، مقایسه عددی انجام می‌دهیم
            if (IsNumericType(actualValue.GetType()) && IsNumericType(filterValue.GetType()))
            {
                return CompareNumericValues(actualValue, filterValue);
            }

            // مقایسه معمولی برای سایر type ها
            return actualValue.Equals(filterValue);
        }

        /// <summary>
        /// متن را برای جست‌وجوی فارسی یکسان می‌کند: ی و ک عربی به فارسی تبدیل
        /// و فاصله، نیم‌فاصله، اعراب و کشیده نادیده گرفته می‌شوند.
        /// </summary>
        private static string NormalizePersianSearchText(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;

            var normalized = value.Normalize(NormalizationForm.FormKC);
            var result = new StringBuilder(normalized.Length);

            foreach (var character in normalized)
            {
                char current = character;
                if (current == '\u064A' || current == '\u0649') current = '\u06CC'; // ي، ى -> ی
                if (current == '\u0643') current = '\u06A9'; // ك -> ک

                var category = CharUnicodeInfo.GetUnicodeCategory(current);
                if (char.IsWhiteSpace(current) || current == '\u200C' || current == '\u200D' ||
                    current == '\u0640' || category == UnicodeCategory.NonSpacingMark)
                {
                    continue;
                }

                result.Append(current);
            }

            return result.ToString();
        }

        /// <summary>
        /// مقایسه دو مقدار عددی
        /// </summary>
        private bool CompareNumericValues(object value1, object value2)
        {
            try
            {
                // تبدیل هر دو به decimal برای مقایسه دقیق
                decimal decimal1 = Convert.ToDecimal(value1, CultureInfo.InvariantCulture);
                decimal decimal2 = Convert.ToDecimal(value2, CultureInfo.InvariantCulture);

                return decimal1 == decimal2;
            }
            catch
            {
                // اگر تبدیل ناموفق بود، از ToString استفاده می‌کنیم
                return value1.ToString() == value2.ToString();
            }
        }

        /// <summary>
        /// چک کردن اینکه آیا یک Type عددی است
        /// </summary>
        private bool IsNumericType(Type type)
        {
            if (type == null) return false;

            // Handle Nullable types
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                type = Nullable.GetUnderlyingType(type);
            }

            // Handle object type
            if (type == typeof(object))
            {
                return false;
            }

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// دریافت مقدار یک property از طریق Reflection
        /// </summary>
        private object GetPropValue(object obj, string propName)
        {
            if (obj == null || string.IsNullOrEmpty(propName))
                return null;

            try
            {
                var property = obj.GetType().GetProperty(propName);
                return property?.GetValue(obj, null);
            }
            catch
            {
                return null;
            }
        }
    }
}
