using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Sharprompt;

/// <summary>
/// Stores model binders. Source generators register binders automatically at startup; custom registrations replace an existing binder for the same type.
/// </summary>
public static class ModelBinderRegistry
{
    private static readonly ConcurrentDictionary<Type, object> s_binders = new();

    /// <summary>
    /// Registers a binder that does not observe cancellation itself.
    /// </summary>
    /// <typeparam name="T">The model type.</typeparam>
    /// <param name="binder">The non-null action that populates the model.</param>
    public static void Register<T>(Action<T> binder) where T : notnull
    {
        ArgumentNullException.ThrowIfNull(binder);

        Register<T>((model, _) => binder(model));
    }

    /// <summary>
    /// Registers a binder that can observe prompt cancellation.
    /// </summary>
    /// <typeparam name="T">The model type.</typeparam>
    /// <param name="binder">The non-null action that populates the model.</param>
    public static void Register<T>(Action<T, CancellationToken> binder) where T : notnull
    {
        ArgumentNullException.ThrowIfNull(binder);

        s_binders[typeof(T)] = binder;
    }
    /// <summary>
    /// Retrieves the registered binder for a model type without constructing a reflection-based fallback.
    /// </summary>
    /// <typeparam name="T">The stored, converted, or selected value type.</typeparam>
    /// <param name="binder">Receives the registered binder, or null when the type has not been registered.</param>
    /// <returns>True when a binder is registered for the type; otherwise false.</returns>
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
