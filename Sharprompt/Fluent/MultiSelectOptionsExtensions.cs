using System;
using System.Collections.Generic;

namespace Sharprompt.Fluent;

public static class MultiSelectOptionsExtensions
{
    public static MultiSelectOptions<T> WithMessage<T>(this MultiSelectOptions<T> options, string message)
    {
        options.Message = message;

        return options;
    }

    public static MultiSelectOptions<T> WithItems<T>(this MultiSelectOptions<T> options, IEnumerable<T> items)
    {
        options.Items = items;

        return options;
    }

    public static MultiSelectOptions<T> WithDefaultValues<T>(this MultiSelectOptions<T> options, IEnumerable<T> defaultValues)
    {
        options.DefaultValues = defaultValues;

        return options;
    }

    public static MultiSelectOptions<T> WithPageSize<T>(this MultiSelectOptions<T> options, int pageSize)
    {
        options.PageSize = pageSize;

        return options;
    }

    public static MultiSelectOptions<T> WithMinimum<T>(this MultiSelectOptions<T> options, int minimum)
    {
        options.Minimum = minimum;

        return options;
    }

    public static MultiSelectOptions<T> WithMaximum<T>(this MultiSelectOptions<T> options, int maximum)
    {
        options.Maximum = maximum;

        return options;
    }

    public static MultiSelectOptions<T> WithTextSelector<T>(this MultiSelectOptions<T> options, Func<T, string> textSelector)
    {
        options.TextSelector = textSelector;

        return options;
    }

    public static MultiSelectOptions<T> WithPagination<T>(this MultiSelectOptions<T> options, Func<int, int, int, string> pagination)
    {
        options.Pagination = pagination;

        return options;
    }
}
