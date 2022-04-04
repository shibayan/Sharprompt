using System;
using System.Collections.Generic;

using Sharprompt.Internal;
using Sharprompt.Strings;

namespace Sharprompt;

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
        Pagination ??= (count, current, total) => string.Format(Resource.Message_Pagination, count, current, total);

        _ = Message ?? throw new ArgumentNullException(nameof(Message));
        _ = Items ?? throw new ArgumentNullException(nameof(Items));

        if (Minimum < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(Minimum), string.Format(Resource.Validation_Minimum_OutOfRange, Minimum));
        }

        if (Maximum < Minimum)
        {
            throw new ArgumentOutOfRangeException(nameof(Maximum), string.Format(Resource.Validation_Maximum_OutOfRange, Maximum, Minimum));
        }
    }
}
