using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Sharprompt
{
    public static class Validators
    {
        public static Func<object, ValidationResult> Required()
        {
            return input =>
            {
                if (input == null)
                {
                    return new ValidationResult("Value is required");
                }

                if (input is string strValue && string.IsNullOrEmpty(strValue))
                {
                    return new ValidationResult("Value is required");
                }

                return ValidationResult.Success;
            };
        }

        public static Func<object, ValidationResult> MinLength(int length)
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

                return new ValidationResult("Value is too short");
            };
        }

        public static Func<object, ValidationResult> MaxLength(int length)
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

                return new ValidationResult("Value is too long");
            };
        }

        public static Func<object, ValidationResult> RegularExpression(string pattern)
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

                return new ValidationResult("Value is not match pattern");
            };
        }
    }
}
