using System.Collections.Generic;

namespace Sharprompt.Internal
{
    internal interface IDataSourceProvider
    {
        IEnumerable<T> GetItems<T>();
    }
}
