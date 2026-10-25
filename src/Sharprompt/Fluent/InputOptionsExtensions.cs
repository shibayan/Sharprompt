using System;
using System.ComponentModel.DataAnnotations;

namespace Sharprompt.Fluent;

/// <summary>
/// Fluent configuration methods for InputOptions.
/// </summary>
public static class InputOptionsExtensions
{
    /// <summary>
    /// Sets Message and returns the same options instance.
    /// </summary>
    /// <typeparam name="T">The input or item type.</typeparam>
    /// <param name="options">The options instance to update.</param>
    /// <param name="message">The prompt message.</param>
    /// <returns>The same options instance for chaining.</returns>
    public static InputOptions<T> WithMessage<T>(this InputOptions<T> options, string message)
    {
        options.Message = message;

        return options;
    }

    /// <summary>
    /// Sets Placeholder and returns the same options instance.
    /// </summary>
    /// <typeparam name="T">The input or item type.</typeparam>
    /// <param name="options">The options instance to update.</param>
    /// <param name="placeholder">The hint shown for empty input.</param>
    /// <returns>The same options instance for chaining.</returns>
    public static InputOptions<T> WithPlaceholder<T>(this InputOptions<T> options, string placeholder)
    {
        options.Placeholder = placeholder;

        return options;
    }

    /// <summary>
    /// Sets DefaultValue and returns the same options instance.
    /// </summary>
    /// <typeparam name="T">The input or item type.</typeparam>
    /// <param name="options">The options instance to update.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>The same options instance for chaining.</returns>
    public static InputOptions<T> WithDefaultValue<T>(this InputOptions<T> options, T defaultValue)
    {
        options.DefaultValue = defaultValue;

        return options;
    }

    /// <summary>
    /// Adds validators to the options and returns the same instance.
    /// </summary>
    /// <typeparam name="T">The input or item type.</typeparam>
    /// <param name="options">The options instance to update.</param>
    /// <param name="validators">Validators returning null on success or a ValidationResult on failure.</param>
    /// <returns>The same options instance for chaining.</returns>
    public static InputOptions<T> AddValidators<T>(this InputOptions<T> options, params Func<object?, ValidationResult?>[] validators)
    {
        foreach (var validator in validators)
        {
            options.Validators.Add(validator);
        }

        return options;
    }
}
