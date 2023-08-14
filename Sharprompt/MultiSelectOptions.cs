using System;
using System.Collections.Generic;
using System.Linq;

using Sharprompt.Internal;
using Sharprompt.Strings;

namespace Sharprompt;

public class MultiSelectOptions<T> where T : notnull
{
    public MultiSelectOptions()
    {
        if (typeof(T).IsAssignableTo(typeof(Enum)))
        {
            Items = EnumHelper<T>.GetValues();
            TextSelector = EnumHelper<T>.GetDisplayName;
        }
    }

    public string Message { get; set; } = null!;

    public IEnumerable<T> Items { get; set; } = null!;

    public IEnumerable<T> DefaultValues { get; set; } = Enumerable.Empty<T>();

    public int PageSize { get; set; } = int.MaxValue;

    public int Minimum { get; set; } = 1;

    public int Maximum { get; set; } = int.MaxValue;

    public Func<T, string> TextSelector { get; set; } = x => x.ToString()!;

    public Func<int, int, int, string> Pagination { get; set; } = (count, current, total) => string.Format(Resource.Message_Pagination, count, current, total);

    public bool LoopingSelection { get; set; } = true;

    internal void EnsureOptions()
    {
        ArgumentNullException.ThrowIfNull(Message);
        ArgumentNullException.ThrowIfNull(Items);
        ArgumentNullException.ThrowIfNull(DefaultValues);
        ArgumentNullException.ThrowIfNull(TextSelector);
        ArgumentNullException.ThrowIfNull(Pagination);

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
