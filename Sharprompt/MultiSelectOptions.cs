using System;
using System.Collections.Generic;

using Sharprompt.Internal;

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

        public Func<T, string> TextSelector { get; set; }

        public Func<int, int, int, string> Pagination { get; set; }

        internal void EnsureOptions()
        {
            if (Items is null && typeof(T).IsEnum)
            {
                Items = EnumHelper<T>.GetValues();
            }

            TextSelector ??= typeof(T).IsEnum ? EnumHelper<T>.GetDisplayName : x => x.ToString();
            Pagination ??= (count, current, total) => $"({count} items, {current}/{total} pages)";

            _ = Items ?? throw new ArgumentNullException(nameof(Items));

            if (Minimum < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(Minimum), $"The minimum ({Minimum}) is not valid");
            }

            if (Maximum < Minimum)
            {
                throw new ArgumentOutOfRangeException(nameof(Maximum), $"The maximum ({Maximum}) is not valid when minimum is set to ({Minimum})");
            }
        }
    }
}
