using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Sharprompt.Internal;

internal class NullItemsProvider : IItemsProvider
{
    public IEnumerable<T> GetItems<T>(PropertyInfo targetPropertyInfo) where T : notnull => Enumerable.Empty<T>();

    public static IItemsProvider Instance { get; } = new NullItemsProvider();
}
