using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sharprompt;

/// <summary>
/// Settings for a masked text prompt. The returned string is not encrypted.
/// </summary>
public class PasswordOptions : PromptOptions
{
    /// <summary>
    /// The hint shown when the password is empty. Defaults to null.
    /// </summary>
    public string? Placeholder { get; set; }

    /// <summary>
    /// The mask displayed for each entered character. Defaults to "*"; cannot be null.
    /// </summary>
    public string PasswordChar { get; set; } = "*";

    /// <summary>
    /// Validators applied to the password. Return ValidationResult.Success (null) to accept it, or a ValidationResult to show an error and retry.
    /// </summary>
    public IList<Func<object?, ValidationResult?>> Validators { get; } = [];

    /// <summary>
    /// Checks that the prompt message and password mask are not null.
    /// </summary>
    /// <exception cref="ArgumentNullException">A required setting is null.</exception>
    internal override void EnsureOptions()
    {
        base.EnsureOptions();

        ArgumentNullException.ThrowIfNull(PasswordChar);
    }
}
