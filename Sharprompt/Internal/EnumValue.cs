using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Sharprompt.Internal
{
    internal class EnumValue<T> where T : Enum
    {
        private EnumValue(T value)
        {
            var name = value.ToString();

            var fieldInfo = typeof(T).GetField(name);

            var displayAttribute = fieldInfo?.GetCustomAttribute<DisplayAttribute>();

            DisplayName = displayAttribute?.Name ?? name;
            Order = displayAttribute?.Order ?? int.MaxValue;
            Value = value;
        }

        public string DisplayName { get; }
        public int Order { get; set; }
        public T Value { get; set; }

        public static IEnumerable<EnumValue<T>> GetValues()
        {
            var values = (T[])Enum.GetValues(typeof(T));

            return values.Select(x => new EnumValue<T>(x))
                         .OrderBy(x => x.Order)
                         .ToArray();
        }
    }
}
