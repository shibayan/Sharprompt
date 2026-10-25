using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sharprompt.Fluent;

/// <summary>
/// Fluent configuration methods for ListOptions.
/// </summary>
public static class ListOptionsExtensions
{
    /// <summary>
    /// Sets Message and returns the same options instance.
    /// </summary>
    /// <typeparam name="T">The input or item type.</typeparam>
    /// <param name="options">The options instance to update.</param>
    /// <param name="message">The prompt message.</param>
    /// <returns>The same options instance for chaining.</returns>
    public static ListOptions<T> WithMessage<T>(this ListOptions<T> options, string message) where T : notnull
    {
        options.Message = message;

        return options;
    }

    /// <summary>
    /// Sets DefaultValues and returns the same options instance.
    /// </summary>
    /// <typeparam name="T">The input or item type.</typeparam>
    /// <param name="options">The options instance to update.</param>
    /// <param name="defaultValues">The initially selected or prepopulated items.</param>
    /// <returns>The same options instance for chaining.</returns>
    public static ListOptions<T> WithDefaultValues<T>(this ListOptions<T> options, IEnumerable<T> defaultValues) where T : notnull
    {
        options.DefaultValues = defaultValues;

        return options;
    }

    /// <summary>
    /// Sets Minimum and returns the same options instance.
    /// </summary>
    /// <typeparam name="T">The input or item type.</typeparam>
    /// <param name="options">The options instance to update.</param>
    /// <param name="minimum">The minimum item count; use 0 to allow an empty result.</param>
    /// <returns>The same options instance for chaining.</returns>
    public static ListOptions<T> WithMinimum<T>(this ListOptions<T> options, int minimum) where T : notnull
    {
        options.Minimum = minimum;

        return options;
    }

    /// <summary>
    /// Sets Maximum and returns the same options instance.
    /// </summary>
    /// <typeparam name="T">The input or item type.</typeparam>
    /// <param name="options">The options instance to update.</param>
    /// <param name="maximum">The maximum item count, at least the minimum.</param>
    /// <returns>The same options instance for chaining.</returns>
    public static ListOptions<T> WithMaximum<T>(this ListOptions<T> options, int maximum) where T : notnull
    {
        options.Maximum = maximum;

        return options;
    }

    /// <summary>
    /// Adds validators to the options and returns the same instance.
    /// </summary>
    /// <typeparam name="T">The input or item type.</typeparam>
    /// <param name="options">The options instance to update.</param>
    /// <param name="validators">Validators returning null on success or a ValidationResult on failure.</param>
    /// <returns>The same options instance for chaining.</returns>
    public static ListOptions<T> AddValidators<T>(this ListOptions<T> options, params Func<object?, ValidationResult?>[] validators) where T : notnull
    {
        foreach (var validator in validators)
        {
            options.Validators.Add(validator);
        }

        return options;
    }
}
