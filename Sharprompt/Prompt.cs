using System;
using System.Collections.Generic;

namespace Sharprompt
{
    public static class Prompt
    {
        public static T Input<T>(string message, object defaultValue = null, IList<Func<object, ValidationError>> validators = null)
        {
            return new Input<T>(message, defaultValue, validators).Start();
        }

        public static string Password(string message, IList<Func<object, ValidationError>> validators = null)
        {
            return new Password(message, validators).Start();
        }

        public static bool Confirm(string message, bool? defaultValue = null)
        {
            return new Confirm(message, defaultValue).Start();
        }

        public static T Select<T>(string message, IEnumerable<T> items, object defaultValue = null, int pageSize = 10, Func<T, string> valueSelector = null)
        {
            return new Select<T>(message, items, defaultValue, pageSize, valueSelector ?? (x => x.ToString())).Start();
        }
        public static IEnumerable<T> ComboBox<T>(string message, IEnumerable<T> items, object defaultValue = null, int pageSize = 10, int limit = -1, int min = 1, Func<T, string> valueSelector = null)
        {
            return new ComboBox<T>(message, items, limit, min, pageSize, valueSelector ?? (x => x.ToString())).Start();
        }
    }
}