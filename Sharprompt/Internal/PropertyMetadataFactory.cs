using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Sharprompt.Internal;

internal static class PropertyMetadataFactory
{
    public static IReadOnlyList<PropertyMetadata> Create<T>(T model)
    {
        return typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .Where(x => x.CanWrite)
                        .Select(x => new PropertyMetadata(model, x))
                        .Where(x => !x.BindIgnore)
                        .OrderBy(x => x.Order)
                        .ToArray();
    }
}
