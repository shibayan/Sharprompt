using System;
using System.Collections.Generic;
using System.Threading;

namespace Sharprompt
{
    public static class Prompt
    {
        public static T Input<T>(string message, object defaultValue = null, IList<Func<object, ValidationError>> validators = null, CancellationToken? cancellationToken = null)
        {
            return new Input<T>(message, defaultValue, validators).Start(cancellationToken);
        }

        public static string Password(string message, IList<Func<object, ValidationError>> validators = null, CancellationToken? cancellationToken = null)
        {
            return new Password(message, validators).Start(cancellationToken);
        }

        public static bool Confirm(string message, bool? defaultValue = null, CancellationToken? cancellationToken = null)
        {
            return new Confirm(message, defaultValue).Start(cancellationToken);
        }

        public static T Select<T>(string message, IEnumerable<T> items, object defaultValue = null, int pageSize = 10, Func<T, string> valueSelector = null, CancellationToken? cancellationToken = null)
        {
            return new Select<T>(message, items, defaultValue, pageSize, valueSelector ?? (x => x.ToString())).Start(cancellationToken);
        }
        public static IEnumerable<T> MultiSelect<T>(string message, IEnumerable<T> items, object defaultValue = null, int pageSize = 10, int limit = -1, int min = 1, Func<T, string> valueSelector = null, CancellationToken? cancellationToken = null)
        {
            return new MultiSelect<T>(message, items, limit, min, pageSize, valueSelector ?? (x => x.ToString())).Start(cancellationToken);
        }
    }
}