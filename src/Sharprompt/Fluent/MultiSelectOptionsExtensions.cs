using System;
using System.Collections.Generic;

namespace Sharprompt.Fluent;

/// <summary>
/// Fluent configuration methods for MultiSelectOptions.
/// </summary>
public static class MultiSelectOptionsExtensions
{
    /// <summary>
    /// Sets Message and returns the same options instance.
    /// </summary>
    /// <typeparam name="T">The input or item type.</typeparam>
    /// <param name="options">The options instance to update.</param>
    /// <param name="message">The prompt message.</param>
    /// <returns>The same options instance for chaining.</returns>
    public static MultiSelectOptions<T> WithMessage<T>(this MultiSelectOptions<T> options, string message) where T : notnull
    {
        options.Message = message;

        return options;
    }

    /// <summary>
    /// Sets Items and returns the same options instance.
    /// </summary>
    /// <typeparam name="T">The input or item type.</typeparam>
    /// <param name="options">The options instance to update.</param>
    /// <param name="items">The available items.</param>
    /// <returns>The same options instance for chaining.</returns>
    public static MultiSelectOptions<T> WithItems<T>(this MultiSelectOptions<T> options, IEnumerable<T> items) where T : notnull
    {
        options.Items = items;

        return options;
    }

    /// <summary>
    /// Sets DefaultValues and returns the same options instance.
    /// </summary>
    /// <typeparam name="T">The input or item type.</typeparam>
    /// <param name="options">The options instance to update.</param>
    /// <param name="defaultValues">The initially selected or prepopulated items.</param>
    /// <returns>The same options instance for chaining.</returns>
    public static MultiSelectOptions<T> WithDefaultValues<T>(this MultiSelectOptions<T> options, IEnumerable<T> defaultValues) where T : notnull
    {
        options.DefaultValues = defaultValues;

        return options;
    }

    /// <summary>
    /// Sets PageSize and returns the same options instance.
    /// </summary>
    /// <typeparam name="T">The input or item type.</typeparam>
    /// <param name="options">The options instance to update.</param>
    /// <param name="pageSize">The requested page size, limited by terminal height.</param>
    /// <returns>The same options instance for chaining.</returns>
    public static MultiSelectOptions<T> WithPageSize<T>(this MultiSelectOptions<T> options, int pageSize) where T : notnull
    {
        options.PageSize = pageSize;

        return options;
    }

    /// <summary>
    /// Sets Minimum and returns the same options instance.
    /// </summary>
    /// <typeparam name="T">The input or item type.</typeparam>
    /// <param name="options">The options instance to update.</param>
    /// <param name="minimum">The minimum item count; use 0 to allow an empty result.</param>
    /// <returns>The same options instance for chaining.</returns>
    public static MultiSelectOptions<T> WithMinimum<T>(this MultiSelectOptions<T> options, int minimum) where T : notnull
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
    public static MultiSelectOptions<T> WithMaximum<T>(this MultiSelectOptions<T> options, int maximum) where T : notnull
    {
        options.Maximum = maximum;

        return options;
    }

    /// <summary>
    /// Sets TextSelector and returns the same options instance.
    /// </summary>
    /// <typeparam name="T">The input or item type.</typeparam>
    /// <param name="options">The options instance to update.</param>
    /// <param name="textSelector">Formats items for display and filtering.</param>
    /// <returns>The same options instance for chaining.</returns>
    public static MultiSelectOptions<T> WithTextSelector<T>(this MultiSelectOptions<T> options, Func<T, string> textSelector) where T : notnull
    {
        options.TextSelector = textSelector;

        return options;
    }

    /// <summary>
    /// Sets Pagination and returns the same options instance.
    /// </summary>
    /// <typeparam name="T">The input or item type.</typeparam>
    /// <param name="options">The options instance to update.</param>
    /// <param name="pagination">Formats the total item count, one-based current page, and total page count.</param>
    /// <returns>The same options instance for chaining.</returns>
    public static MultiSelectOptions<T> WithPagination<T>(this MultiSelectOptions<T> options, Func<int, int, int, string> pagination) where T : notnull
    {
        options.Pagination = pagination;

        return options;
    }
}
