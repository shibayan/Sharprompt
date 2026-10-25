using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

using Sharprompt.Strings;

namespace Sharprompt;

/// <summary>
/// Creates validators for prompt input. Validators return ValidationResult.Success (null) for accepted values.
/// </summary>
public static class Validators
{
    /// <summary>
    /// Rejects null and empty strings. Whitespace-only strings are accepted.
    /// </summary>
    /// <param name="errorMessage">The error to display; null uses the localized built-in message.</param>
    /// <returns>A validator for an options Validators collection or the validators argument.</returns>
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

    /// <summary>
    /// Rejects strings shorter than the specified character count. Non-string values are accepted.
    /// </summary>
    /// <param name="length">The character-count limit.</param>
    /// <param name="errorMessage">The error to display; null uses the localized built-in message.</param>
    /// <returns>A validator for an options Validators collection or the validators argument.</returns>
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

    /// <summary>
    /// Rejects strings longer than the specified character count. Non-string values are accepted.
    /// </summary>
    /// <param name="length">The character-count limit.</param>
    /// <param name="errorMessage">The error to display; null uses the localized built-in message.</param>
    /// <returns>A validator for an options Validators collection or the validators argument.</returns>
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

    /// <summary>
    /// Requires a matching substring using Regex.IsMatch. Anchor the pattern to require a whole-string match. Non-string values are accepted.
    /// </summary>
    /// <param name="pattern">The regular expression to match.</param>
    /// <param name="errorMessage">The error to display; null uses the localized built-in message.</param>
    /// <returns>A validator for an options Validators collection or the validators argument.</returns>
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
