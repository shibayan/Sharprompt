using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Sharprompt.Internal
{
    internal class EnumValue<T> : IEquatable<EnumValue<T>> where T : Enum
    {
        private EnumValue(T value)
        {
            var name = value.ToString();
            var displayAttribute = typeof(T).GetField(name)?.GetCustomAttribute<DisplayAttribute>();

            DisplayName = displayAttribute?.Name ?? name;
            Order = displayAttribute?.GetOrder() ?? int.MaxValue;
            Value = value;
        }

        public string DisplayName { get; }
        public int Order { get; }
        public T Value { get; }

        public override bool Equals(object obj) => Equals(obj as EnumValue<T>);

        public bool Equals(EnumValue<T> other)
        {
            if (other == null)
            {
                return false;
            }

            return EqualityComparer<T>.Default.Equals(Value, other.Value);
        }

        public override int GetHashCode() => Value.GetHashCode();

        public static implicit operator EnumValue<T>(T value)
        {
            return new EnumValue<T>(value);
        }

        public static IEnumerable<EnumValue<T>> GetValues()
        {
            var values = (T[])Enum.GetValues(typeof(T));

            return values.Select(x => new EnumValue<T>(x))
                         .OrderBy(x => x.Order)
                         .ToArray();
        }
    }
}
