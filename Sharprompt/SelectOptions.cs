using System;
using System.Collections.Generic;

using Sharprompt.Strings;

namespace Sharprompt;

public class SelectOptions<T> where T : notnull
{
    public string Message { get; set; } = null!;

    public IEnumerable<T> Items { get; set; } = null!;

    public T? DefaultValue { get; set; }

    public int PageSize { get; set; } = int.MaxValue;

    public Func<T, string> TextSelector { get; set; } = x => x.ToString()!;

    public Func<int, int, int, string> Pagination { get; set; } = (count, current, total) => string.Format(Resource.Message_Pagination, count, current, total);

    internal void EnsureOptions()
    {
        ArgumentNullException.ThrowIfNull(Message);
        ArgumentNullException.ThrowIfNull(Items);
        ArgumentNullException.ThrowIfNull(TextSelector);
        ArgumentNullException.ThrowIfNull(Pagination);
    }
}
