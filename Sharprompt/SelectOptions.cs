using System;
using System.Collections.Generic;

namespace Sharprompt
{
    public class SelectOptions<T>
    {
        public string Message { get; set; }

        public IEnumerable<T> Items { get; set; }

        public object DefaultValue { get; set; }

        public int? PageSize { get; set; }

        public Func<T, string> TextSelector { get; set; } = x => x.ToString();

        public Func<int, int, int, string> Pagination { get; set; } = (count, current, total) => $"({count} items, {current}/{total} pages)";

        public SelectOptions<T> SetMessage(string message)
        {
            Message = message;

            return this;
        }

        public SelectOptions<T> SetItems(IEnumerable<T> items)
        {
            Items = items;

            return this;
        }

        public SelectOptions<T> SetDefaultValue(object defaultValue)
        {
            DefaultValue = defaultValue;

            return this;
        }

        public SelectOptions<T> SetPageSize(int? pageSize)
        {
            PageSize = pageSize;

            return this;
        }

        public SelectOptions<T> SetTextSelector(Func<T, string> textSelector)
        {
            TextSelector = textSelector;

            return this;
        }

        public SelectOptions<T> SetPagination(Func<int, int, int, string> pagination)
        {
            Pagination = pagination;

            return this;
        }
    }
}
