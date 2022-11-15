using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Sharprompt.Internal;

internal static class PropertyMetadataFactory
{
    public static IReadOnlyList<PropertyMetadata> Create<T>(T model) where T : notnull
    {
        return typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .Where(x => x.CanWrite && x.GetCustomAttribute<BindIgnoreAttribute>() is null)
                        .Select(x => new PropertyMetadata(model, x))
                        .OrderBy(x => x.Order)
                        .ToArray();
    }
}
