using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Sharprompt;

public static class ModelBinderRegistry
{
    private static readonly ConcurrentDictionary<Type, object> s_binders = new();

    public static void Register<T>(Action<T> binder) where T : notnull
    {
        ArgumentNullException.ThrowIfNull(binder);

        Register<T>((model, _) => binder(model));
    }

    public static void Register<T>(Action<T, CancellationToken> binder) where T : notnull
    {
        ArgumentNullException.ThrowIfNull(binder);

        s_binders[typeof(T)] = binder;
    }

    internal static bool TryGetBinder<T>(out Action<T, CancellationToken>? binder) where T : notnull
    {
        if (s_binders.TryGetValue(typeof(T), out var obj))
        {
            binder = (Action<T, CancellationToken>)obj;
            return true;
        }

        binder = null;
        return false;
    }
}
