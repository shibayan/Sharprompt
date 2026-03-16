using System;
using System.Collections.Generic;

namespace Sharprompt.Fluent;

public static class MultiSelectOptionsExtensions
{
    public static MultiSelectOptions<T> WithMessage<T>(this MultiSelectOptions<T> options, string message) where T : notnull
    {
        options.Message = message;

        return options;
    }

    public static MultiSelectOptions<T> WithItems<T>(this MultiSelectOptions<T> options, IEnumerable<T> items) where T : notnull
    {
        options.Items = items;

        return options;
    }

    public static MultiSelectOptions<T> WithDefaultValues<T>(this MultiSelectOptions<T> options, IEnumerable<T> defaultValues) where T : notnull
    {
        options.DefaultValues = defaultValues;

        return options;
    }

    public static MultiSelectOptions<T> WithPageSize<T>(this MultiSelectOptions<T> options, int pageSize) where T : notnull
    {
        options.PageSize = pageSize;

        return options;
    }

    public static MultiSelectOptions<T> WithMinimum<T>(this MultiSelectOptions<T> options, int minimum) where T : notnull
    {
        options.Minimum = minimum;

        return options;
    }

    public static MultiSelectOptions<T> WithMaximum<T>(this MultiSelectOptions<T> options, int maximum) where T : notnull
    {
        options.Maximum = maximum;

        return options;
    }

    public static MultiSelectOptions<T> WithTextSelector<T>(this MultiSelectOptions<T> options, Func<T, string> textSelector) where T : notnull
    {
        options.TextSelector = textSelector;

        return options;
    }

    public static MultiSelectOptions<T> WithPagination<T>(this MultiSelectOptions<T> options, Func<int, int, int, string> pagination) where T : notnull
    {
        options.Pagination = pagination;

        return options;
    }
}
