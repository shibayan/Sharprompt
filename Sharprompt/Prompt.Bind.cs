using System;

namespace Sharprompt;

public static partial class Prompt
{
    public static T Bind<T>() where T : notnull, new()
    {
        var model = new T();

        return Bind(model);
    }

    public static T Bind<T>(T model) where T : notnull
    {
        if (!ModelBinderRegistry.TryGetBinder<T>(out var binder))
        {
            throw new InvalidOperationException($"No SourceGenerator-based binder is registered for type '{typeof(T).FullName}'. Apply [PromptBindable] attribute to the type.");
        }

        binder!(model);

        return model;
    }
}
