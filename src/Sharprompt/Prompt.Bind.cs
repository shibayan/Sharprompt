using System;
using System.Threading;

namespace Sharprompt;

public static partial class Prompt
{
    public static T Bind<T>() where T : notnull, new()
    {
        return Bind<T>(CancellationToken.None);
    }

    public static T Bind<T>(CancellationToken cancellationToken) where T : notnull, new()
    {
        var model = new T();

        return Bind(model, cancellationToken);
    }

    public static T Bind<T>(T model) where T : notnull
    {
        return Bind(model, CancellationToken.None);
    }

    public static T Bind<T>(T model, CancellationToken cancellationToken) where T : notnull
    {
        if (!ModelBinderRegistry.TryGetBinder<T>(out var binder))
        {
            throw new InvalidOperationException($"No SourceGenerator-based binder is registered for type '{typeof(T).FullName}'. Apply [PromptBindable] attribute to the type.");
        }

        binder!(model, cancellationToken);

        return model;
    }
}
