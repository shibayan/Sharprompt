using System;
using System.ComponentModel.DataAnnotations;

namespace Sharprompt.Fluent;

/// <summary>
/// Fluent configuration methods for PasswordOptions.
/// </summary>
public static class PasswordOptionsExtensions
{
    /// <summary>
    /// Sets Message and returns the same options instance.
    /// </summary>
    /// <param name="options">The options instance to update.</param>
    /// <param name="message">The prompt message.</param>
    /// <returns>The same options instance for chaining.</returns>
    public static PasswordOptions WithMessage(this PasswordOptions options, string message)
    {
        options.Message = message;

        return options;
    }

    /// <summary>
    /// Sets Placeholder and returns the same options instance.
    /// </summary>
    /// <param name="options">The options instance to update.</param>
    /// <param name="placeholder">The hint shown for empty input.</param>
    /// <returns>The same options instance for chaining.</returns>
    public static PasswordOptions WithPlaceholder(this PasswordOptions options, string placeholder)
    {
        options.Placeholder = placeholder;

        return options;
    }

    /// <summary>
    /// Sets PasswordChar and returns the same options instance.
    /// </summary>
    /// <param name="options">The options instance to update.</param>
    /// <param name="passwordChar">The mask shown for each entered password character.</param>
    /// <returns>The same options instance for chaining.</returns>
    public static PasswordOptions WithPasswordChar(this PasswordOptions options, string passwordChar)
    {
        options.PasswordChar = passwordChar;

        return options;
    }

    /// <summary>
    /// Adds validators to the options and returns the same instance.
    /// </summary>
    /// <param name="options">The options instance to update.</param>
    /// <param name="validators">Validators returning null on success or a ValidationResult on failure.</param>
    /// <returns>The same options instance for chaining.</returns>
    public static PasswordOptions AddValidators(this PasswordOptions options, params Func<object?, ValidationResult?>[] validators)
    {
        foreach (var validator in validators)
        {
            options.Validators.Add(validator);
        }

        return options;
    }
}
