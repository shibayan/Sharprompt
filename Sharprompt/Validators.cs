﻿using System;
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
                    return new ValidationResult(errorMessage ?? Prompt.Messages.Required);
                }

                if (input is string strValue && string.IsNullOrEmpty(strValue))
                {
                    return new ValidationResult(errorMessage ?? Prompt.Messages.Required);
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

                return new ValidationResult(errorMessage ?? Prompt.Messages.MinLength);
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

                return new ValidationResult(errorMessage ?? Prompt.Messages.MaxLength);
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

                return new ValidationResult(errorMessage ?? Prompt.Messages.NoMatchRegex);
            };
        }
    }
}
