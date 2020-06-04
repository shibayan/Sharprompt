using System;
using System.Collections.Generic;

using Sharprompt.Forms;

namespace Sharprompt
{
    public static class Prompt
    {
        public static T Input<T>(string message, object defaultValue = null, IList<Func<object, ValidationError>> validators = null)
        {
            using var form = new Input<T>(message, defaultValue, validators);

            return form.Start();
        }

        public static string Password(string message, IList<Func<object, ValidationError>> validators = null)
        {
            using var form = new Password(message, validators);

            return form.Start();
        }

        public static bool Confirm(string message, bool? defaultValue = null)
        {
            using var form = new Confirm(message, defaultValue);

            return form.Start();
        }

        public static T Select<T>(string message, IEnumerable<T> items, object defaultValue = null, int pageSize = 10, Func<T, string> valueSelector = null)
        {
            using var form = new Select<T>(message, items, defaultValue, pageSize, valueSelector ?? (x => x.ToString()));

            return form.Start();
        }

        public static IEnumerable<T> MultiSelect<T>(string message, IEnumerable<T> items, object defaultValue = null, int pageSize = 10, int limit = -1, int min = 1, Func<T, string> valueSelector = null)
        {
            using var form = new MultiSelect<T>(message, items, limit, min, pageSize, valueSelector ?? (x => x.ToString()));

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
