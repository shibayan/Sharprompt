using System;

namespace Sharprompt.Internal
{
    internal static class TypeHelper<T>
    {
        private static readonly Type _targetType = typeof(T);
        private static readonly Type _underlyingType = Nullable.GetUnderlyingType(typeof(T));

        public static bool IsValueType => _targetType.IsValueType && _underlyingType is null;

        public static T ConvertTo(object value) => (T)Convert.ChangeType(value, _underlyingType ?? _targetType);
    }
}
