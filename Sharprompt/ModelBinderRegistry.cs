using System;
using System.Collections.Concurrent;

namespace Sharprompt;

public static class ModelBinderRegistry
{
    private static readonly ConcurrentDictionary<Type, object> s_binders = new();

    public static void Register<T>(Action<T> binder) where T : notnull
    {
        s_binders[typeof(T)] = binder;
    }

    internal static bool TryGetBinder<T>(out Action<T>? binder) where T : notnull
    {
        if (s_binders.TryGetValue(typeof(T), out var obj))
        {
            binder = (Action<T>)obj;
            return true;
        }

        binder = null;
        return false;
    }
}
