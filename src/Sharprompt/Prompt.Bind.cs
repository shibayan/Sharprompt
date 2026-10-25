using System;
using System.Threading;

namespace Sharprompt;

public static partial class Prompt
{
    /// <summary>
    /// Creates a model and prompts for its properties using its registered binder.
    /// </summary>
    /// <typeparam name="T">A class or record class annotated with PromptBindable and accessible settable properties. Bind&lt;T&gt;() also requires a public parameterless constructor.</typeparam>
    /// <returns>The populated model.</returns>
    /// <exception cref="InvalidOperationException">No binder is registered. Apply PromptBindable and resolve the source generator SHARP warnings; the default driver also requires terminal input and output.</exception>
    /// <exception cref="PromptCanceledException">The user cancels and ThrowExceptionOnCancel is true.</exception>
    /// <remarks>Init-only properties, including positional record properties, cannot be bound. Use get/set properties in the record body. No runtime reflection binder is provided.</remarks>
    public static T Bind<T>() where T : notnull, new()
    {
        return Bind<T>(CancellationToken.None);
    }

    /// <summary>
    /// Creates a model and prompts for its properties using its registered binder.
    /// </summary>
    /// <typeparam name="T">A class or record class annotated with PromptBindable and accessible settable properties. Bind&lt;T&gt;() also requires a public parameterless constructor.</typeparam>
    /// <param name="cancellationToken">Cancels before prompting or while waiting for input.</param>
    /// <returns>The populated model.</returns>
    /// <exception cref="InvalidOperationException">No binder is registered. Apply PromptBindable and resolve the source generator SHARP warnings; the default driver also requires terminal input and output.</exception>
    /// <exception cref="PromptCanceledException">The user cancels and ThrowExceptionOnCancel is true.</exception>
    /// <exception cref="OperationCanceledException">The cancellation token is canceled.</exception>
    /// <remarks>Init-only properties, including positional record properties, cannot be bound. Use get/set properties in the record body. No runtime reflection binder is provided.</remarks>
    public static T Bind<T>(CancellationToken cancellationToken) where T : notnull, new()
    {
        var model = new T();

        return Bind(model, cancellationToken);
    }

    /// <summary>
    /// Prompts for the properties of an existing model using its registered binder and returns that instance.
    /// </summary>
    /// <typeparam name="T">A class or record class annotated with PromptBindable and accessible settable properties. Bind&lt;T&gt;() also requires a public parameterless constructor.</typeparam>
    /// <param name="model">The instance to populate; current property values supply prompt defaults except for passwords.</param>
    /// <returns>The populated model.</returns>
    /// <exception cref="InvalidOperationException">No binder is registered. Apply PromptBindable and resolve the source generator SHARP warnings; the default driver also requires terminal input and output.</exception>
    /// <exception cref="PromptCanceledException">The user cancels and ThrowExceptionOnCancel is true.</exception>
    /// <remarks>Init-only properties, including positional record properties, cannot be bound. Use get/set properties in the record body. No runtime reflection binder is provided.</remarks>
    public static T Bind<T>(T model) where T : notnull
    {
        return Bind(model, CancellationToken.None);
    }

    /// <summary>
    /// Prompts for the properties of an existing model using its registered binder and returns that instance.
    /// </summary>
    /// <typeparam name="T">A class or record class annotated with PromptBindable and accessible settable properties. Bind&lt;T&gt;() also requires a public parameterless constructor.</typeparam>
    /// <param name="model">The instance to populate; current property values supply prompt defaults except for passwords.</param>
    /// <param name="cancellationToken">Cancels before prompting or while waiting for input.</param>
    /// <returns>The populated model.</returns>
    /// <exception cref="InvalidOperationException">No binder is registered. Apply PromptBindable and resolve the source generator SHARP warnings; the default driver also requires terminal input and output.</exception>
    /// <exception cref="PromptCanceledException">The user cancels and ThrowExceptionOnCancel is true.</exception>
    /// <exception cref="OperationCanceledException">The cancellation token is canceled.</exception>
    /// <remarks>Init-only properties, including positional record properties, cannot be bound. Use get/set properties in the record body. No runtime reflection binder is provided.</remarks>
    public static T Bind<T>(T model, CancellationToken cancellationToken) where T : notnull
    {
        if (!ModelBinderRegistry.TryGetBinder<T>(out var binder))
        {
            throw new InvalidOperationException($"No SourceGenerator-based binder is registered for type '{typeof(T).FullName}'. Apply [PromptBindable] attribute to the type. If it is already applied, check the SHARP warnings that the Sharprompt source generator reported for the type, such as SHARP014 when it has no bindable properties.");
        }

        cancellationToken.ThrowIfCancellationRequested();

        binder!(model, cancellationToken);

        return model;
    }
}
