using System;
using System.Collections.Generic;

using Sharprompt.Strings;

namespace Sharprompt;

/// <summary>
/// Settings for selecting multiple items. Enum items and display names are populated automatically.
/// </summary>
/// <typeparam name="T">The input or item type.</typeparam>
public class MultiSelectOptions<T> : PromptOptions where T : notnull
{
    /// <summary>
    /// Creates settings and loads enum metadata when available.
    /// </summary>
    public MultiSelectOptions()
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
    /// The initially selected items. Defaults to an empty sequence.
    /// </summary>
    public IEnumerable<T> DefaultValues { get; set; } = [];

    /// <summary>
    /// The requested number of items per page, limited by terminal height. Defaults to int.MaxValue.
    /// </summary>
    public int PageSize { get; set; } = int.MaxValue;

    /// <summary>
    /// The minimum number of selected items. Defaults to 1; set to 0 to allow an empty selection.
    /// </summary>
    public int Minimum { get; set; } = 1;

    /// <summary>
    /// The maximum number of selected items. Defaults to int.MaxValue and must be at least Minimum.
    /// </summary>
    public int Maximum { get; set; } = int.MaxValue;

    /// <summary>
    /// Formats an item for display and filtering. Defaults to enum display names when available, otherwise ToString(). The original items are returned.
    /// </summary>
    public Func<T, string> TextSelector { get; set; } = x => x.ToString()!;

    /// <summary>
    /// Formats the pagination hint from the total item count, one-based current page, and total page count.
    /// </summary>
    public Func<int, int, int, string> Pagination { get; set; } = (count, current, total) => string.Format(Resource.Message_Pagination, count, current, total);

    /// <summary>
    /// Whether navigation wraps at the beginning and end of the list. Defaults to true.
    /// </summary>
    public bool LoopingSelection { get; set; } = true;

    /// <summary>
    /// Checks required selection settings and verifies nonnegative, ordered selection limits.
    /// </summary>
    /// <exception cref="ArgumentNullException">A required setting is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The minimum is negative or the maximum is less than the minimum.</exception>
    internal override void EnsureOptions()
    {
        base.EnsureOptions();

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
