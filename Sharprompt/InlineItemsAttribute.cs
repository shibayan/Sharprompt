using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Sharprompt.Internal;

namespace Sharprompt;

[AttributeUsage(AttributeTargets.Property)]
public sealed class InlineItemsAttribute : Attribute, IItemsProvider
{
    public InlineItemsAttribute(params object[] items)
    {
        _items = items;
    }

    private readonly object[] _items;

    public IEnumerable<T> GetItems<T>(PropertyInfo targetPropertyInfo) => _items.Cast<T>();
}
