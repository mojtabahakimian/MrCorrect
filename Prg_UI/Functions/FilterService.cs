using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Functions
{
    public class FilterService<T>
    {
        private readonly List<(string ColumnName, object FilterValue, bool IsExclusion, bool IsExactMatch, bool IsCustomTextSearch)> cumulativeFilters =
            new List<(string ColumnName, object FilterValue, bool IsExclusion, bool IsExactMatch, bool IsCustomTextSearch)>();

        /// <summary>
        /// اضافه کردن فیلتر جدید به لیست فیلترهای فعال
        /// </summary>
        /// <param name="columnName">نام ستون</param>
        /// <param name="filterValue">مقدار فیلتر (می‌تواند string، numeric، یا null باشد)</param>
        /// <param name="isExclusion">آیا فیلتر exclusion است؟</param>
        /// <param name="isExactMatch">آیا باید دقیقاً برابر باشد؟ (در غیر این صورت Contains)</param>
        /// <param name="isCustomTextSearch">
        /// آیا این فیلتر از نوع «جستجوی سفارشی متنی» است؟ در این حالت filterValue باید string باشد،
        /// متن جستجو به کلمات مستقل تقسیم می‌شود و پس از یکسان‌سازی حروف عربی/فارسی و نیم‌فاصله،
        /// هر کلمه به‌صورت جداگانه (نه لزوماً پشت سر هم) در مقدار واقعی جستجو می‌شود.
        /// </param>
        public void AddFilter(string columnName, object filterValue, bool isExclusion = false, bool isExactMatch = false, bool isCustomTextSearch = false)
        {
            cumulativeFilters.Add((columnName, filterValue, isExclusion, isExactMatch, isCustomTextSearch));
        }

        /// <summary>
        /// اضافه کردن فیلتر «پالودن با : سفارشی متنی» — نسخه‌ی مقاوم در برابر نیم‌فاصله
        /// و اختلاف حروف عربی/فارسی (ی و ک) که مشکل جستجوهای ناموفق را حل می‌کند.
        /// </summary>
        /// <param name="columnName">نام ستون (MappingName)</param>
        /// <param name="searchText">متن آزاد وارد شده توسط کاربر (می‌تواند شامل چند کلمه باشد)</param>
        /// <param name="isExclusion">آیا فیلتر باید معکوس (Does Not Contain) باشد؟</param>
        public void AddCustomTextFilter(string columnName, string searchText, bool isExclusion = false)
        {
            AddFilter(columnName, searchText, isExclusion: isExclusion, isExactMatch: false, isCustomTextSearch: true);
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

            foreach (var (columnName, filterValue, isExclusion, isExactMatch, isCustomTextSearch) in cumulativeFilters)
            {
                // دریافت مقدار واقعی property (نه ToString آن)
                var actualValue = GetPropValue(item, columnName);

                // اعمال فیلتر — برای جستجوی سفارشی متنی از منطق اختصاصی چندکلمه‌ای استفاده می‌شود
                bool matchResult = isCustomTextSearch
                    ? EvaluateCustomTextSearch(actualValue, filterValue as string)
                    : EvaluateFilter(actualValue, filterValue, isExactMatch);

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
        private bool EvaluateFilter(object actualValue, object filterValue, bool isExactMatch)
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
                // یکسان‌سازی حروف عربی/فارسی و نیم‌فاصله، سپس Trim
                filterString = NormalizePersianText(filterString).Trim();

                // چک کردن blank values
                if (string.IsNullOrWhiteSpace(filterString))
                {
                    return string.IsNullOrWhiteSpace(actualValue.ToString());
                }

                // تبدیل actualValue به string برای مقایسه متنی (با همان یکسان‌سازی)
                string actualString = NormalizePersianText(actualValue.ToString() ?? string.Empty).Trim();

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
        /// ارزیابی «جستجوی سفارشی متنی»: متن جستجو به کلمات مستقل تقسیم می‌شود؛
        /// پس از یکسان‌سازی حروف عربی/فارسی (ی، ک)، تبدیل نیم‌فاصله به فاصله و یکسان‌سازی ارقام،
        /// همه‌ی کلمات باید (نه لزوماً پشت‌سرهم و با فاصله‌ی دقیق) در متن واقعی یافت شوند.
        /// این روش مشکلاتی مانند «چک 93600» که در داده به‌صورت «...193600بانک...» ذخیره شده را حل می‌کند.
        /// </summary>
        private bool EvaluateCustomTextSearch(object actualValue, string searchText)
        {
            string normalizedSearch = NormalizePersianText(searchText ?? string.Empty).Trim();

            // اگر متن جستجو خالی است، فقط مقادیر خالی/تهی را match کن
            if (string.IsNullOrWhiteSpace(normalizedSearch))
            {
                return actualValue == null || string.IsNullOrWhiteSpace(actualValue.ToString());
            }

            if (actualValue == null) return false;

            string normalizedActual = NormalizePersianText(actualValue.ToString() ?? string.Empty).Trim();

            // تقسیم متن جستجو به کلمات مستقل (فاصله معمولی، بعد از تبدیل نیم‌فاصله به فاصله)
            var tokens = normalizedSearch.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length == 0) return true;

            foreach (var token in tokens)
            {
                if (normalizedActual.IndexOf(token, StringComparison.OrdinalIgnoreCase) < 0)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// یکسان‌سازی متن فارسی/عربی برای رفع مشکلات جستجو:
        /// - کاف عربی (ك، U+0643) → کاف فارسی (ک، U+06A9)
        /// - یای عربی (ي، U+064A) و الف مقصوره (ى، U+0649) → یای فارسی (ی، U+06CC)
        /// - تای مربوطه عربی (ة، U+0629) → های فارسی (ه، U+0647)
        /// - نیم‌فاصله / ZWNJ (U+200C) → فاصله معمولی
        /// - ارقام عربی و فارسی (٠-٩ و ۰-۹) → ارقام انگلیسی (0-9)
        /// </summary>
        private string NormalizePersianText(string input)
        {
            if (string.IsNullOrEmpty(input)) return input ?? string.Empty;

            var sb = new StringBuilder(input.Length);

            foreach (char c in input)
            {
                switch (c)
                {
                    case '\u0643': sb.Append('\u06A9'); break; // ك → ک
                    case '\u064A': sb.Append('\u06CC'); break; // ي → ی
                    case '\u0649': sb.Append('\u06CC'); break; // ى → ی
                    case '\u0629': sb.Append('\u0647'); break; // ة → ه
                    case '\u200C': sb.Append(' '); break;      // نیم‌فاصله → فاصله معمولی

                    // ارقام عربی (Arabic-Indic)
                    case '\u0660': sb.Append('0'); break;
                    case '\u0661': sb.Append('1'); break;
                    case '\u0662': sb.Append('2'); break;
                    case '\u0663': sb.Append('3'); break;
                    case '\u0664': sb.Append('4'); break;
                    case '\u0665': sb.Append('5'); break;
                    case '\u0666': sb.Append('6'); break;
                    case '\u0667': sb.Append('7'); break;
                    case '\u0668': sb.Append('8'); break;
                    case '\u0669': sb.Append('9'); break;

                    // ارقام فارسی (Extended Arabic-Indic)
                    case '\u06F0': sb.Append('0'); break;
                    case '\u06F1': sb.Append('1'); break;
                    case '\u06F2': sb.Append('2'); break;
                    case '\u06F3': sb.Append('3'); break;
                    case '\u06F4': sb.Append('4'); break;
                    case '\u06F5': sb.Append('5'); break;
                    case '\u06F6': sb.Append('6'); break;
                    case '\u06F7': sb.Append('7'); break;
                    case '\u06F8': sb.Append('8'); break;
                    case '\u06F9': sb.Append('9'); break;

                    default:
                        sb.Append(c);
                        break;
                }
            }

            return sb.ToString();
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