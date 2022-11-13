using System.Collections.Generic;
using System.Reflection;

namespace Sharprompt.Internal;

internal interface IItemsProvider
{
    IEnumerable<T> GetItems<T>(PropertyInfo targetPropertyInfo) where T : notnull;
}
