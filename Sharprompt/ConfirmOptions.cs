using System;

namespace Sharprompt;

public class ConfirmOptions
{
    public string Message { get; set; } = null!;

    public bool? DefaultValue { get; set; }

    internal void EnsureOptions()
    {
        ArgumentNullException.ThrowIfNull(Message);
    }
}
