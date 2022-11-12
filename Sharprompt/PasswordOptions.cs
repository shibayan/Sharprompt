using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sharprompt;

public class PasswordOptions
{
    public string Message { get; set; } = null!;

    public string? Placeholder { get; set; }

    public string PasswordChar { get; set; } = "*";

    public IList<Func<object?, ValidationResult?>> Validators { get; } = new List<Func<object?, ValidationResult?>>();

    internal void EnsureOptions()
    {
        ArgumentNullException.ThrowIfNull(Message);
        ArgumentNullException.ThrowIfNull(PasswordChar);
    }
}
