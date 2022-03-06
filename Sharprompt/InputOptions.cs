using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sharprompt;

public class InputOptions<T>
{
    public string Message { get; set; }

    public string Placeholder { get; set; }

    public object DefaultValue { get; set; }

    public IList<Func<object, ValidationResult>> Validators { get; } = new List<Func<object, ValidationResult>>();

    internal void EnsureOptions()
    {
        _ = Message ?? throw new ArgumentNullException(nameof(Message));
    }
}
