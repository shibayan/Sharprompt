using System;
using System.Collections.Generic;

namespace Sharprompt
{
    public class MultiSelectOptions<T>
    {
        public string Message { get; set; }

        public IEnumerable<T> Items { get; set; }

        public IEnumerable<T> DefaultValues { get; set; }

        public int? PageSize { get; set; }

        public int Minimum { get; set; } = 1;

        public int Maximum { get; set; } = int.MaxValue;

        public Func<T, string> TextSelector { get; set; } = x => x.ToString();

        public Func<int, int, int, string> Pagination { get; set; } = (count, current, total) => $"({count} items, {current}/{total} pages)";

        public MultiSelectOptions<T> SetMessage(string message)
        {
            Message = message;

            return this;
        }

        public MultiSelectOptions<T> SetItems(IEnumerable<T> items)
        {
            Items = items;

            return this;
        }

        public MultiSelectOptions<T> SetDefaultValues(IEnumerable<T> defaultValues)
        {
            DefaultValues = defaultValues;

            return this;
        }

        public MultiSelectOptions<T> SetPageSize(int? pageSize)
        {
            PageSize = pageSize;

            return this;
        }

        public MultiSelectOptions<T> SetMinimum(int minimum)
        {
            Minimum = minimum;

            return this;
        }

        public MultiSelectOptions<T> SetMaximum(int maximum)
        {
            Maximum = maximum;

            return this;
        }

        public MultiSelectOptions<T> SetTextSelector(Func<T, string> textSelector)
        {
            TextSelector = textSelector;

            return this;
        }

        public MultiSelectOptions<T> SetPagination(Func<int, int, int, string> pagination)
        {
            Pagination = pagination;

            return this;
        }
    }
}
