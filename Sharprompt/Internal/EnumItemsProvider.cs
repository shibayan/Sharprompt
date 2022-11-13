using System.Collections.Generic;
using System.Reflection;

namespace Sharprompt.Internal;

internal class EnumItemsProvider : IItemsProvider
{
    public IEnumerable<T> GetItems<T>(PropertyInfo targetPropertyInfo) where T : notnull => EnumHelper<T>.GetValues();
}
