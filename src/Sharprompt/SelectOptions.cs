using System;
using System.Collections.Generic;

using Sharprompt.Strings;

namespace Sharprompt;

public class SelectOptions<T> : PromptOptions where T : notnull
{
    public SelectOptions()
    {
        if (EnumMetadataRegistry.TryGet<T>(out var values, out var textSelector))
        {
            Items = values;
            TextSelector = textSelector;
        }
    }

    public IEnumerable<T> Items { get; set; } = null!;

    public object? DefaultValue { get; set; }

    public int PageSize { get; set; } = int.MaxValue;

    public Func<T, string> TextSelector { get; set; } = x => x.ToString()!;

    public Func<int, int, int, string> Pagination { get; set; } = (count, current, total) => string.Format(Resource.Message_Pagination, count, current, total);

    public bool LoopingSelection { get; set; } = true;

    /// <summary>
    /// Creates an item from the text typed when Enter cannot resolve a listed item.
    /// When <see langword="null"/> (the default), <c>Select</c> only returns items from <see cref="Items"/>.
    /// </summary>
    public Func<string, T>? CreateCustomItem { get; set; }

    internal override void EnsureOptions()
    {
        base.EnsureOptions();

        ArgumentNullException.ThrowIfNull(Items);
        ArgumentNullException.ThrowIfNull(TextSelector);
        ArgumentNullException.ThrowIfNull(Pagination);
    }
}
