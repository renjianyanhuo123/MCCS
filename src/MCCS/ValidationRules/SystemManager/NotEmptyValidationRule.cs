using System.Globalization;
using System.Windows.Controls;

namespace MCCS.ValidationRules.SystemManager
{
    public class NotEmptyValidationRule : ValidationRule
    {
        public string ErrorMessage { get; set; } = "此字段不能为空!";

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var input = value as string;

            return string.IsNullOrWhiteSpace(input) ? new ValidationResult(false, ErrorMessage) : ValidationResult.ValidResult;
        }
    }
}
