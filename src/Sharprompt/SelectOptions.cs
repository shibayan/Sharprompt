using System;
using System.Collections.Generic;

using Sharprompt.Strings;

namespace Sharprompt;

/// <summary>
/// Settings for selecting one item. Enum items and display names are populated automatically.
/// </summary>
/// <typeparam name="T">The input or item type.</typeparam>
public class SelectOptions<T> : PromptOptions where T : notnull
{
    /// <summary>
    /// Creates settings and loads enum metadata when available.
    /// </summary>
    public SelectOptions()
    {
        if (EnumMetadataRegistry.TryGet<T>(out var values, out var textSelector))
        {
            Items = values;
            TextSelector = textSelector;
        }
    }

    /// <summary>
    /// The available items. Required for types without registered enum metadata.
    /// </summary>
    public IEnumerable<T> Items { get; set; } = null!;

    /// <summary>
    /// The initially highlighted item. Must be compatible with T; null leaves the selection unset. A single matching item can still be accepted with Enter.
    /// </summary>
    public object? DefaultValue { get; set; }

    /// <summary>
    /// The requested number of items per page, limited by terminal height. Defaults to int.MaxValue.
    /// </summary>
    public int PageSize { get; set; } = int.MaxValue;

    /// <summary>
    /// Formats an item for display and filtering. Defaults to enum display names when available, otherwise ToString(). The original item is returned.
    /// </summary>
    public Func<T, string> TextSelector { get; set; } = x => x.ToString()!;

    /// <summary>
    /// Formats the pagination hint from the total item count, one-based current page, and total page count.
    /// </summary>
    public Func<int, int, int, string> Pagination { get; set; } = (count, current, total) => string.Format(Resource.Message_Pagination, count, current, total);

    /// <summary>
    /// Whether Up/Down navigation wraps within the current page instead of moving between pages. Defaults to true.
    /// </summary>
    /// <remarks>
    /// When false, Up/Down navigation crosses page boundaries and still wraps between the first and last pages.
    /// Left/Right page navigation also wraps between the first and last pages.
    /// </remarks>
    public bool LoopingSelection { get; set; } = true;

    /// <summary>
    /// Checks the required message, item sequence, and display and pagination delegates.
    /// </summary>
    /// <exception cref="ArgumentNullException">A required setting is null.</exception>
    internal override void EnsureOptions()
    {
        base.EnsureOptions();

        ArgumentNullException.ThrowIfNull(Items);
        ArgumentNullException.ThrowIfNull(TextSelector);
        ArgumentNullException.ThrowIfNull(Pagination);
    }
}
