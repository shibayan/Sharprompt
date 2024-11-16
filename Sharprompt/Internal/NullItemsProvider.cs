using System.Collections.Generic;
using System.Reflection;

namespace Sharprompt.Internal;

internal class NullItemsProvider : IItemsProvider
{
    public IEnumerable<T> GetItems<T>(PropertyInfo targetPropertyInfo) where T : notnull => [];

    public static IItemsProvider Instance { get; } = new NullItemsProvider();
}
