using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sharprompt;

public class PasswordOptions : PromptOptions
{
    public string? Placeholder { get; set; }

    public string PasswordChar { get; set; } = "*";

    public IList<Func<object?, ValidationResult?>> Validators { get; } = [];

    internal override void EnsureOptions()
    {
        base.EnsureOptions();

        ArgumentNullException.ThrowIfNull(PasswordChar);
    }
}
