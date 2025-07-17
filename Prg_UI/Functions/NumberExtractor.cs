using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Functions
{
    public static class NumberExtractor
    {
        public static string[] ExtractNumbersArray(string text)
        {
            //Phind:

            // Regular expression pattern to match numbers
            var numberPattern = @"-?\d+(?:\.\d+)?";

            try
            {
                // Use Regex.Matches to find all occurrences of numbers in the text
                var matches = Regex.Matches(text, numberPattern);

                // Convert MatchCollection to string array
                var result = new string[matches.Count];
                for (int i = 0; i < matches.Count; i++)
                {
                    result[i] = matches[i].Value;
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during number extraction: {ex.Message}");
                return Array.Empty<string>();
            }
        }
        public static string ExtractNumbersLine(string text)
        {
            //Phind:

            // Regular expression pattern to match numbers
            var numberPattern = @"-?\d+(?:\.\d+)?";

            try
            {
                // Use Regex.Matches to find all occurrences of numbers in the text
                var matches = Regex.Matches(text, numberPattern);

                // Convert MatchCollection to string array
                string result = "";
                for (int i = 0; i < matches.Count; i++)
                {
                    result += matches[i].Value;
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during number extraction: {ex.Message}");
                return string.Empty;
            }
        }

        #region Advance_Extractor
        //public class ExtractedNumber
        //{
        //    public string OriginalText { get; set; }
        //    public double? NumericValue { get; set; }
        //    public bool IsInteger { get; set; }
        //    public bool IsNegative { get; set; }
        //    public bool HasDecimalPoint { get; set; }
        //    public int StartIndex { get; set; }
        //    public int Length { get; set; }
        //    public string Unit { get; set; }
        //}

        //public static List<ExtractedNumber> ExtractNumbersFromText(string input,
        //    bool includeNegatives = true,
        //    bool includeDecimals = true,
        //    bool detectUnits = true,
        //    CultureInfo culture = null)
        //{
        //    if (string.IsNullOrEmpty(input))
        //        return new List<ExtractedNumber>();

        //    culture ??= CultureInfo.InvariantCulture;
        //    var results = new List<ExtractedNumber>();
        //    var currentNumber = new StringBuilder();
        //    var currentUnit = new StringBuilder();
        //    bool isInNumber = false;
        //    bool hasDecimalPoint = false;
        //    bool isNegative = false;
        //    int startIndex = 0;

        //    for (int i = 0; i < input.Length; i++)
        //    {
        //        char c = input[i];
        //        char? nextChar = i < input.Length - 1 ? input[i + 1] : (char?)null;
        //        char? prevChar = i > 0 ? input[i - 1] : (char?)null;

        //        // Handle negative numbers
        //        if (c == '-' && includeNegatives)
        //        {
        //            if (!isInNumber && (prevChar == null || !char.IsLetterOrDigit(prevChar.Value)))
        //            {
        //                if (nextChar.HasValue && (char.IsDigit(nextChar.Value) || nextChar.Value == '.'))
        //                {
        //                    isNegative = true;
        //                    isInNumber = true;
        //                    startIndex = i;
        //                    currentNumber.Append(c);
        //                    continue;
        //                }
        //            }
        //        }

        //        // Handle decimal points
        //        if (c == '.' && includeDecimals)
        //        {
        //            if (isInNumber && !hasDecimalPoint && nextChar.HasValue && char.IsDigit(nextChar.Value))
        //            {
        //                hasDecimalPoint = true;
        //                currentNumber.Append(c);
        //                continue;
        //            }
        //        }

        //        // Handle digits
        //        if (char.IsDigit(c))
        //        {
        //            if (!isInNumber)
        //            {
        //                startIndex = i;
        //                isInNumber = true;
        //            }
        //            currentNumber.Append(c);
        //            continue;
        //        }

        //        // Handle potential units
        //        if (detectUnits && isInNumber && char.IsLetter(c))
        //        {
        //            currentUnit.Append(c);

        //            // Check next character to see if we should continue collecting unit
        //            if (!nextChar.HasValue || !char.IsLetter(nextChar.Value))
        //            {
        //                TryAddNumber(currentNumber.ToString(), currentUnit.ToString(), startIndex);
        //                ResetState();
        //            }
        //            continue;
        //        }

        //        // Handle number completion
        //        if (isInNumber)
        //        {
        //            TryAddNumber(currentNumber.ToString(), currentUnit.ToString(), startIndex);
        //            ResetState();
        //        }
        //    }

        //    // Handle case where number is at end of string
        //    if (isInNumber)
        //    {
        //        TryAddNumber(currentNumber.ToString(), currentUnit.ToString(), startIndex);
        //    }

        //    return results;

        //    void ResetState()
        //    {
        //        currentNumber.Clear();
        //        currentUnit.Clear();
        //        isInNumber = false;
        //        hasDecimalPoint = false;
        //        isNegative = false;
        //    }

        //    void TryAddNumber(string numberStr, string unit, int index)
        //    {
        //        if (string.IsNullOrEmpty(numberStr))
        //            return;

        //        if (double.TryParse(numberStr, NumberStyles.Any, culture, out double value))
        //        {
        //            var extractedNumber = new ExtractedNumber
        //            {
        //                OriginalText = numberStr,
        //                NumericValue = value,
        //                IsInteger = !numberStr.Contains('.'),
        //                IsNegative = value < 0,
        //                HasDecimalPoint = hasDecimalPoint,
        //                StartIndex = index,
        //                Length = numberStr.Length,
        //                Unit = string.IsNullOrEmpty(unit) ? null : unit
        //            };
        //            results.Add(extractedNumber);
        //        }
        //    }
        //}

        //// Additional helper methods for common use cases
        //public static List<int> ExtractIntegers(string input)
        //{
        //    return ExtractNumbersFromText(input, includeDecimals: false)
        //        .Where(n => n.IsInteger)
        //        .Select(n => (int)n.NumericValue.Value)
        //        .ToList();
        //}

        //public static List<double> ExtractDecimals(string input)
        //{
        //    return ExtractNumbersFromText(input)
        //        .Select(n => n.NumericValue.Value)
        //        .ToList();
        //}

        //public static double? ExtractFirstNumber(string input)
        //{
        //    return ExtractNumbersFromText(input)
        //        .FirstOrDefault()?.NumericValue;
        //}

        //// Validation helper
        //public static bool ValidateNumberFormat(string input,
        //    bool allowNegative = true,
        //    bool allowDecimal = true,
        //    int? maxDecimals = null)
        //{
        //    if (string.IsNullOrEmpty(input))
        //        return false;

        //    var extracted = ExtractNumbersFromText(input,
        //        includeNegatives: allowNegative,
        //        includeDecimals: allowDecimal)
        //        .FirstOrDefault();

        //    if (extracted == null)
        //        return false;

        //    if (maxDecimals.HasValue && extracted.HasDecimalPoint)
        //    {
        //        var decimalPart = extracted.OriginalText.Split('.')[1];
        //        if (decimalPart.Length > maxDecimals.Value)
        //            return false;
        //    }

        //    return true;
        //}
        
        #endregion
    }
}
