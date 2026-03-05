using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sharprompt;

public class InputOptions<T> : PromptOptions
{
    public string? Placeholder { get; set; }

    public object? DefaultValue { get; set; }

    public IList<Func<object?, ValidationResult?>> Validators { get; } = [];
}
