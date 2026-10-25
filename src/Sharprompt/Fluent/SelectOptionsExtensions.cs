using System;
using System.Collections.Generic;

namespace Sharprompt.Fluent;

/// <summary>
/// Fluent configuration methods for SelectOptions.
/// </summary>
public static class SelectOptionsExtensions
{
    /// <summary>
    /// Sets Message and returns the same options instance.
    /// </summary>
    /// <typeparam name="T">The input or item type.</typeparam>
    /// <param name="options">The options instance to update.</param>
    /// <param name="message">The prompt message.</param>
    /// <returns>The same options instance for chaining.</returns>
    public static SelectOptions<T> WithMessage<T>(this SelectOptions<T> options, string message) where T : notnull
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
    public static SelectOptions<T> WithItems<T>(this SelectOptions<T> options, IEnumerable<T> items) where T : notnull
    {
        options.Items = items;

        return options;
    }

    /// <summary>
    /// Sets DefaultValue and returns the same options instance.
    /// </summary>
    /// <typeparam name="T">The input or item type.</typeparam>
    /// <param name="options">The options instance to update.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>The same options instance for chaining.</returns>
    public static SelectOptions<T> WithDefaultValue<T>(this SelectOptions<T> options, T defaultValue) where T : notnull
    {
        options.DefaultValue = defaultValue;

        return options;
    }

    /// <summary>
    /// Sets PageSize and returns the same options instance.
    /// </summary>
    /// <typeparam name="T">The input or item type.</typeparam>
    /// <param name="options">The options instance to update.</param>
    /// <param name="pageSize">The requested page size, limited by terminal height.</param>
    /// <returns>The same options instance for chaining.</returns>
    public static SelectOptions<T> WithPageSize<T>(this SelectOptions<T> options, int pageSize) where T : notnull
    {
        options.PageSize = pageSize;

        return options;
    }

    /// <summary>
    /// Sets TextSelector and returns the same options instance.
    /// </summary>
    /// <typeparam name="T">The input or item type.</typeparam>
    /// <param name="options">The options instance to update.</param>
    /// <param name="textSelector">Formats items for display and filtering.</param>
    /// <returns>The same options instance for chaining.</returns>
    public static SelectOptions<T> WithTextSelector<T>(this SelectOptions<T> options, Func<T, string> textSelector) where T : notnull
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
    public static SelectOptions<T> WithPagination<T>(this SelectOptions<T> options, Func<int, int, int, string> pagination) where T : notnull
    {
        options.Pagination = pagination;

        return options;
    }
}
