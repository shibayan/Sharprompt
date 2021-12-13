using System.Collections.Generic;

namespace Sharprompt.Internal
{
    internal interface IItemsProvider
    {
        IEnumerable<T> GetItems<T>();
    }
}
