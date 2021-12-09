using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Sharprompt.Internal
{
    internal static class EnumHelper<T>
    {
        static EnumHelper()
        {
            var values = (T[])Enum.GetValues(typeof(T));

            foreach (var value in values)
            {
                _cache.Add(value, GetEnumMetadata(value));
            }
        }

        private static readonly Dictionary<T, EnumMetadata> _cache = new();

        private static EnumMetadata GetEnumMetadata(T value)
        {
            var displayAttribute = typeof(T).GetField(value.ToString())?.GetCustomAttribute<DisplayAttribute>();

            return new EnumMetadata
            {
                DisplayName = displayAttribute?.GetName(),
                Order = displayAttribute?.GetOrder()
            };
        }

        public static string GetDisplayName(T value)
        {
            return _cache[value]?.DisplayName ?? value.ToString();
        }

        public static IEnumerable<T> GetValues()
        {
            return _cache.OrderBy(x => x.Value.Order)
                         .Select(x => x.Key)
                         .ToArray();
        }

        private class EnumMetadata
        {
            public string DisplayName { get; set; }
            public int? Order { get; set; }
        }
    }
}
