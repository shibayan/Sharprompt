using System;
using System.Collections.Generic;

namespace Sharprompt.Fluent
{
    public static class MultiSelectOptionsExtensions
    {
        public static MultiSelectOptions<T> SetMessage<T>(this MultiSelectOptions<T> options, string message)
        {
            options.Message = message;

            return options;
        }

        public static MultiSelectOptions<T> SetItems<T>(this MultiSelectOptions<T> options, IEnumerable<T> items)
        {
            options.Items = items;

            return options;
        }

        public static MultiSelectOptions<T> SetDefaultValues<T>(this MultiSelectOptions<T> options, IEnumerable<T> defaultValues)
        {
            options.DefaultValues = defaultValues;

            return options;
        }

        public static MultiSelectOptions<T> SetPageSize<T>(this MultiSelectOptions<T> options, int? pageSize)
        {
            options.PageSize = pageSize;

            return options;
        }

        public static MultiSelectOptions<T> SetMinimum<T>(this MultiSelectOptions<T> options, int minimum)
        {
            options.Minimum = minimum;

            return options;
        }

        public static MultiSelectOptions<T> SetMaximum<T>(this MultiSelectOptions<T> options, int maximum)
        {
            options.Maximum = maximum;

            return options;
        }

        public static MultiSelectOptions<T> SetTextSelector<T>(this MultiSelectOptions<T> options, Func<T, string> textSelector)
        {
            options.TextSelector = textSelector;

            return options;
        }

        public static MultiSelectOptions<T> SetPagination<T>(this MultiSelectOptions<T> options, Func<int, int, int, string> pagination)
        {
            options.Pagination = pagination;

            return options;
        }
    }
}
