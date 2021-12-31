using System;
using System.Collections;
using System.ComponentModel;

namespace Sharprompt.Internal
{
    internal static class TypeHelper
    {
        public static bool IsNullable(Type type) => !type.IsValueType || Nullable.GetUnderlyingType(type) is not null;

        public static bool IsCollection(Type type) => typeof(IEnumerable).IsAssignableFrom(type) && type != typeof(string);
    }

    internal static class TypeHelper<T>
    {
        private static readonly Type _targetType = typeof(T);
        private static readonly Type _underlyingType = Nullable.GetUnderlyingType(typeof(T));

        public static bool IsValueType => _targetType.IsValueType && _underlyingType is null;

        public static T ConvertTo(string value) => (T)TypeDescriptor.GetConverter(_underlyingType ?? _targetType).ConvertFromInvariantString(value);
    }
}
