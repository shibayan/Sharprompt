using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

using Sharprompt.Strings;

namespace Sharprompt;

public static class Validators
{
    public static Func<object?, ValidationResult?> Required(string? errorMessage = default)
    {
        return input =>
        {
            if (input is null)
            {
                return new ValidationResult(errorMessage ?? Resource.Validation_Required);
            }

            if (input is string strValue && string.IsNullOrEmpty(strValue))
            {
                return new ValidationResult(errorMessage ?? Resource.Validation_Required);
            }

            return ValidationResult.Success;
        };
    }

    public static Func<object?, ValidationResult?> MinLength(int length, string? errorMessage = default)
    {
        return input =>
        {
            if (input is not string strValue)
            {
                return ValidationResult.Success;
            }

            if (strValue.Length >= length)
            {
                return ValidationResult.Success;
            }

            return new ValidationResult(errorMessage ?? Resource.Validation_MinLength);
        };
    }

    public static Func<object?, ValidationResult?> MaxLength(int length, string? errorMessage = default)
    {
        return input =>
        {
            if (input is not string strValue)
            {
                return ValidationResult.Success;
            }

            if (strValue.Length <= length)
            {
                return ValidationResult.Success;
            }

            return new ValidationResult(errorMessage ?? Resource.Validation_MaxLength);
        };
    }

    public static Func<object?, ValidationResult?> RegularExpression(string pattern, string? errorMessage = default)
    {
        return input =>
        {
            if (input is not string strValue)
            {
                return ValidationResult.Success;
            }

            if (Regex.IsMatch(strValue, pattern))
            {
                return ValidationResult.Success;
            }

            return new ValidationResult(errorMessage ?? Resource.Validation_RegularExpression);
        };
    }
}
