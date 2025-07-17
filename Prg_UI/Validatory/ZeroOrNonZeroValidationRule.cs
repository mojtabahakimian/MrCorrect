using System.Globalization;
using System.Windows.Controls;

namespace Prg_UI.Validatory
{
    public class ZeroOrNonZeroValidationRule : ValidationRule
    {
        public bool AllowZero { get; set; } = false;
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            double result;
            if (double.TryParse(value.ToString(), out result))
            {
                if (AllowZero) // true
                {
                    if (result >= 0)  //0 or 1
                    {
                        return ValidationResult.ValidResult;
                    }
                }
                else //false
                {
                    if (result > 0)  //1
                    {
                        return ValidationResult.ValidResult;
                    }
                }
            }
            return new ValidationResult(false, "Value cannot be zero.");
        }
    }
}
