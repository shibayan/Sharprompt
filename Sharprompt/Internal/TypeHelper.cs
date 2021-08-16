using System;
using System.ComponentModel;

namespace Sharprompt.Internal
{
    internal static class TypeHelper<T>
    {
        private static readonly Type _targetType = typeof(T);
        private static readonly Type _underlyingType = Nullable.GetUnderlyingType(typeof(T));

        public static bool IsValueType => _targetType.IsValueType && _underlyingType is null;

        public static T ConvertTo(string value) => (T)TypeDescriptor.GetConverter(_underlyingType ?? _targetType).ConvertFromInvariantString(value);
    }
}
