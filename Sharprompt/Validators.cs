using System;
using System.Text.RegularExpressions;

namespace Sharprompt
{
    public static class Validators
    {
        public static Func<object, ValidationError> Required()
        {
            return input =>
            {
                if (input is string strValue && !string.IsNullOrEmpty(strValue))
                {
                    return null;
                }

                return new ValidationError("Value is required");
            };
        }

        public static Func<object, ValidationError> MinLength(int length)
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

                return new ValidationError("Value is too short");
            };
        }

        public static Func<object, ValidationError> MaxLength(int length)
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

                return new ValidationError("Value is too long");
            };
        }

        public static Func<object, ValidationError> RegularExpression(string pattern)
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

                return new ValidationError("Value is not match pattern");
            };
        }
    }
}
