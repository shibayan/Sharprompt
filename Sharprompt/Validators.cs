using System;
using System.Text.RegularExpressions;

namespace Sharprompt
{
    public static class Validators
    {
        public static Func<object, Error> Required()
        {
            return input =>
            {
                if (input is string strValue && !string.IsNullOrEmpty(strValue))
                {
                    return null;
                }

                return new Error("Value is required");
            };
        }

        public static Func<object, Error> MinLength(int length)
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

                return new Error("Value is too short");
            };
        }

        public static Func<object, Error> MaxLength(int length)
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

                return new Error("Value is too long");
            };
        }

        public static Func<object, Error> RegularExpression(string pattern)
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

                return new Error("Value is not match pattern");
            };
        }
    }
}
