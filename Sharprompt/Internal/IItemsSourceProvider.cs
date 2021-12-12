using System.Collections.Generic;

namespace Sharprompt.Internal
{
    internal interface IItemsSourceProvider
    {
        IEnumerable<T> GetItems<T>();
    }
}
