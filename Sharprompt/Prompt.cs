using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

using Sharprompt.Forms;
using Sharprompt.Internal;

namespace Sharprompt
{
    public static class Prompt
    {
        public static T Input<T>(string message, T? defaultValue = default, IReadOnlyList<Func<object?, ValidationResult>>? validators = default) where T : notnull
        {
            using var form = new Input<T>(message, defaultValue, validators ?? Array.Empty<Func<object?, ValidationResult>>());

            return form.Start();
        }

        public static string Password(string message, IReadOnlyList<Func<object?, ValidationResult>>? validators = default)
        {
            using var form = new Password(message, validators ?? Array.Empty<Func<object?, ValidationResult>>());

            return form.Start();
        }

        public static bool Confirm(string message, bool? defaultValue = default)
        {
            using var form = new Confirm(message, defaultValue);

            return form.Start();
        }

        public static T Select<T>(string message, int? pageSize = null, T? defaultValue = default) where T : struct, Enum
        {
            var items = EnumValue<T>.GetValues();

            using var form = new Select<EnumValue<T>>(message, items, pageSize, defaultValue, x => x.DisplayName);

            return form.Start().Value;
        }

        public static T Select<T>(string message, IEnumerable<T> items, int? pageSize = default, T? defaultValue = default, Func<T, string>? valueSelector = default) where T : notnull
        {
            using var form = new Select<T>(message, items, pageSize, defaultValue, valueSelector ?? (x => x.ToString()));

            return form.Start();
        }

        public static IEnumerable<T> MultiSelect<T>(string message, int? pageSize = default, int minimum = 1, int maximum = -1) where T : struct, Enum
        {
            var items = EnumValue<T>.GetValues();

            using var form = new MultiSelect<EnumValue<T>>(message, items, pageSize, minimum, maximum, x => x.DisplayName);

            return form.Start().Select(x => x.Value);
        }

        public static IEnumerable<T> MultiSelect<T>(string message, IEnumerable<T> items, int? pageSize = default, int minimum = 1, int maximum = -1, Func<T, string>? valueSelector = default) where T : notnull
        {
            using var form = new MultiSelect<T>(message, items, pageSize, minimum, maximum, valueSelector ?? (x => x.ToString()));

            return form.Start();
        }

        public static class ColorSchema
        {
            public static ConsoleColor Answer { get; set; } = ConsoleColor.Cyan;
            public static ConsoleColor Select { get; set; } = ConsoleColor.Green;
            public static ConsoleColor DisabledOption { get; set; } = ConsoleColor.DarkCyan;
        }
    }
}
