using System;
using System.Collections.Generic;

using Sharprompt.Internal;

namespace Sharprompt
{
    public class SelectOptions<T>
    {
        public string Message { get; set; }

        public IEnumerable<T> Items { get; set; }

        public object DefaultValue { get; set; }

        public int? PageSize { get; set; }

        public Func<T, string> TextSelector { get; set; }

        public Func<int, int, int, string> Pagination { get; set; }

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

        internal void EnsureOptions()
        {
            if (Items is null && typeof(T).IsEnum)
            {
                Items = EnumHelper<T>.GetValues();
            }

            TextSelector ??= typeof(T).IsEnum ? EnumHelper<T>.GetDisplayName : x => x.ToString();
            Pagination ??= (count, current, total) => $"({count} items, {current}/{total} pages)";

            _ = Items ?? throw new ArgumentNullException(nameof(Items));
        }
    }
}
