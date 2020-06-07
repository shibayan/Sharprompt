using System;
using System.Text.RegularExpressions;

namespace Sharprompt.Validations
{
    public static class Validators
    {
        public static Func<object, ValidationResult> Required()
        {
            return input =>
            {
                if (input is string strValue && !string.IsNullOrEmpty(strValue))
                {
                    return null;
                }

                return new ValidationResult("Value is required");
            };
        }

        public static Func<object, ValidationResult> MinLength(int length)
        {
            return input =>
            {
                if (!(input is string strValue))
                {
                    return null;
                }

                if (strValue.Length >= length)
                {
                    return null;
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
                    return null;
                }

                if (strValue.Length <= length)
                {
                    return null;
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
                    return null;
                }

                if (Regex.IsMatch(strValue, pattern))
                {
                    return null;
                }

                return new ValidationResult("Value is not match pattern");
            };
        }
    }
}
