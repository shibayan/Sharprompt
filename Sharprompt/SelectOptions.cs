using System;
using System.Collections.Generic;

using Sharprompt.Internal;
using Sharprompt.Strings;

namespace Sharprompt;

public class SelectOptions<T>
{
    public string Message { get; set; }

    public IEnumerable<T> Items { get; set; }

    public object DefaultValue { get; set; }

    public int? PageSize { get; set; }

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
    }
}
