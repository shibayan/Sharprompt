using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sharprompt;

/// <summary>
/// Settings for a text prompt that converts input to T using an invariant-culture TypeConverter.
/// </summary>
/// <typeparam name="T">The input or item type.</typeparam>
public class InputOptions<T> : PromptOptions
{
    /// <summary>
    /// The hint shown when the input is empty. Defaults to null.
    /// </summary>
    public string? Placeholder { get; set; }

    /// <summary>
    /// The value accepted by Enter when nothing is typed. Must be compatible with T; null means no default. Convertible primitive values and integral enum values are accepted. Tab inserts it for editing; Backspace dismisses it.
    /// </summary>
    public object? DefaultValue { get; set; }

    /// <summary>
    /// Validators applied to the converted value. Return ValidationResult.Success (null) to accept it, or a ValidationResult to show an error and retry.
    /// </summary>
    public IList<Func<object?, ValidationResult?>> Validators { get; } = [];
}
