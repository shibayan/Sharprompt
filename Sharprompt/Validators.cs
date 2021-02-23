using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Sharprompt
{
    public static class Validators
    {
        public static Func<object, ValidationResult> Required(string errorMessage = null)
        {
            return input =>
            {
                if (input == null)
                {
                    return new ValidationResult(errorMessage ?? "Value is required");
                }

                if (input is string strValue && string.IsNullOrEmpty(strValue))
                {
                    return new ValidationResult(errorMessage ?? "Value is required");
                }

                return ValidationResult.Success;
            };
        }

        public static Func<object, ValidationResult> MinLength(int length, string errorMessage = null)
        {
            return input =>
            {
                if (!(input is string strValue))
                {
                    return ValidationResult.Success;
                }

                if (strValue.Length >= length)
                {
                    return ValidationResult.Success;
                }

                return new ValidationResult(errorMessage ?? "Value is too short");
            };
        }

        public static Func<object, ValidationResult> MaxLength(int length, string errorMessage = null)
        {
            return input =>
            {
                if (!(input is string strValue))
                {
                    return ValidationResult.Success;
                }

                if (strValue.Length <= length)
                {
                    return ValidationResult.Success;
                }

                return new ValidationResult(errorMessage ?? "Value is too long");
            };
        }

        public static Func<object, ValidationResult> RegularExpression(string pattern, string errorMessage = null)
        {
            return input =>
            {
                if (!(input is string strValue))
                {
                    return ValidationResult.Success;
                }

                if (Regex.IsMatch(strValue, pattern))
                {
                    return ValidationResult.Success;
                }

                return new ValidationResult(errorMessage ?? "Value is not match pattern");
            };
        }
    }
}
