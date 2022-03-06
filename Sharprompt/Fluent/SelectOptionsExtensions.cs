using System;
using System.Collections.Generic;

namespace Sharprompt.Fluent;

public static class SelectOptionsExtensions
{
    public static SelectOptions<T> WithMessage<T>(this SelectOptions<T> options, string message)
    {
        options.Message = message;

        return options;
    }

    public static SelectOptions<T> WithItems<T>(this SelectOptions<T> options, IEnumerable<T> items)
    {
        options.Items = items;

        return options;
    }

    public static SelectOptions<T> WithDefaultValue<T>(this SelectOptions<T> options, T defaultValue)
    {
        options.DefaultValue = defaultValue;

        return options;
    }

    public static SelectOptions<T> WithPageSize<T>(this SelectOptions<T> options, int pageSize)
    {
        options.PageSize = pageSize;

        return options;
    }

    public static SelectOptions<T> WithTextSelector<T>(this SelectOptions<T> options, Func<T, string> textSelector)
    {
        options.TextSelector = textSelector;

        return options;
    }

    public static SelectOptions<T> WithPagination<T>(this SelectOptions<T> options, Func<int, int, int, string> pagination)
    {
        options.Pagination = pagination;

        return options;
    }
}
