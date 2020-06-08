using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Sharprompt.Internal
{
    internal static class EnumExtensions
    {
        public static string GetDisplayName<T>(this T value) where T : struct, Enum
        {
            var name = value.ToString();

            var fieldInfo = typeof(T).GetField(name);

            var displayAttribute = fieldInfo?.GetCustomAttribute<DisplayAttribute>();

            return displayAttribute != null ? displayAttribute.Name : name;
        }
    }
}
