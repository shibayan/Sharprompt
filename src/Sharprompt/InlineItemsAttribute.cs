using System;

namespace Sharprompt;

[AttributeUsage(AttributeTargets.Property)]
public sealed class InlineItemsAttribute : Attribute
{
    public InlineItemsAttribute(params object[] items)
    {
        ArgumentNullException.ThrowIfNull(items);
    }
}
