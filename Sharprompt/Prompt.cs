using System;
using System.Collections.Generic;

namespace Sharprompt
{
    public static class Prompt
    {
        public static T Input<T>(string message)
        {
            return new Input<T>(message).Start();
        }

        public static T Input<T>(string message, T defaultValue)
        {
            return new Input<T>(message, defaultValue).Start();
        }

        public static string Password(string message)
        {
            return new Password(message).Start();
        }

        public static bool Confirm(string message)
        {
            return new Confirm(message).Start();
        }

        public static bool Confirm(string message, bool defaultValue)
        {
            return new Confirm(message, defaultValue).Start();
        }

        public static T Select<T>(string message, IReadOnlyList<T> items)
        {
            return new Select<T>(message, items).Start();
        }

        public static T Select<T>(string message, IReadOnlyList<T> items, Func<T, string> labelSelector)
        {
            return new Select<T>(message, items, labelSelector).Start();
        }
    }
}