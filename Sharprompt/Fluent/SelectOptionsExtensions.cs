using System;
using System.Collections.Generic;

namespace Sharprompt.Fluent
{
    public static class SelectOptionsExtensions
    {
        public static SelectOptions<T> SetMessage<T>(this SelectOptions<T> options, string message)
        {
            options.Message = message;

            return options;
        }

        public static SelectOptions<T> SetItems<T>(this SelectOptions<T> options, IEnumerable<T> items)
        {
            options.Items = items;

            return options;
        }

        public static SelectOptions<T> SetDefaultValue<T>(this SelectOptions<T> options, object defaultValue)
        {
            options.DefaultValue = defaultValue;

            return options;
        }

        public static SelectOptions<T> SetPageSize<T>(this SelectOptions<T> options, int? pageSize)
        {
            options.PageSize = pageSize;

            return options;
        }

        public static SelectOptions<T> SetTextSelector<T>(this SelectOptions<T> options, Func<T, string> textSelector)
        {
            options.TextSelector = textSelector;

            return options;
        }

        public static SelectOptions<T> SetPagination<T>(this SelectOptions<T> options, Func<int, int, int, string> pagination)
        {
            options.Pagination = pagination;

            return options;
        }
    }
}
