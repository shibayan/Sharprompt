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

        public static T Select<T>(string message, IReadOnlyList<T> items, object defaultValue = null, Func<T, string> labelSelector = null)
        {
            return new Select<T>(message, items, defaultValue, labelSelector).Start();
        }
    }
}