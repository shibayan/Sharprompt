using System;
using System.Collections.Generic;

namespace Sharprompt
{
    public static class Prompt
    {
        public static T Input<T>(string message, object defaultValue = null)
        {
            return new Input<T>(message, defaultValue).Start();
        }

        public static string Password(string message)
        {
            return new Password(message).Start();
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