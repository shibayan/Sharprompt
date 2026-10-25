using System;

namespace Sharprompt;

/// <summary>
/// Provides constant items for a model property bound with Select or MultiSelect. Values must match the item type exactly.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class InlineItemsAttribute : Attribute
{
    /// <summary>
    /// Specifies the constant items used by a selection prompt.
    /// </summary>
    /// <param name="items">The available items. Cannot be null.</param>
    public InlineItemsAttribute(params object[] items)
    {
        ArgumentNullException.ThrowIfNull(items);
    }
}
