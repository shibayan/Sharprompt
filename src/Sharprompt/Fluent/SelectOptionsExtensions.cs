using System;
using System.Collections.Generic;

namespace Sharprompt.Fluent;

public static class SelectOptionsExtensions
{
    public static SelectOptions<T> WithMessage<T>(this SelectOptions<T> options, string message) where T : notnull
    {
        options.Message = message;

        return options;
    }

    public static SelectOptions<T> WithItems<T>(this SelectOptions<T> options, IEnumerable<T> items) where T : notnull
    {
        options.Items = items;

        return options;
    }

    public static SelectOptions<T> WithDefaultValue<T>(this SelectOptions<T> options, T defaultValue) where T : notnull
    {
        options.DefaultValue = defaultValue;

        return options;
    }

    public static SelectOptions<T> WithPageSize<T>(this SelectOptions<T> options, int pageSize) where T : notnull
    {
        options.PageSize = pageSize;

        return options;
    }

    public static SelectOptions<T> WithTextSelector<T>(this SelectOptions<T> options, Func<T, string> textSelector) where T : notnull
    {
        options.TextSelector = textSelector;

        return options;
    }

    public static SelectOptions<T> WithPagination<T>(this SelectOptions<T> options, Func<int, int, int, string> pagination) where T : notnull
    {
        options.Pagination = pagination;

        return options;
    }
}
