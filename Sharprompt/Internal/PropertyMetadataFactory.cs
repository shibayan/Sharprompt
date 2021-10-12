using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Sharprompt.Internal
{

    internal static class PropertyMetadataFactory
    {
        public static IReadOnlyList<PropertyMetadata> Create<T>(T model)
        {
            return typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                            .Select(x => new PropertyMetadata(model, x))
                            .OrderBy(x => x.Order)
                            .ToArray();
        }
    }
}
