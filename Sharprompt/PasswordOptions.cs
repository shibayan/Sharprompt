using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sharprompt;

public class PasswordOptions
{
    public string Message { get; set; }

    public string Placeholder { get; set; }

    public string PasswordChar { get; set; } = "*";

    public IList<Func<object, ValidationResult>> Validators { get; } = new List<Func<object, ValidationResult>>();

    internal void EnsureOptions()
    {
        _ = Message ?? throw new ArgumentNullException(nameof(Message));
        _ = PasswordChar ?? throw new ArgumentNullException(nameof(PasswordChar));
    }
}
