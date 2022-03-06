using System;

namespace Sharprompt;

public class ConfirmOptions
{
    public string Message { get; set; }

    public bool? DefaultValue { get; set; }

    internal void EnsureOptions()
    {
        _ = Message ?? throw new ArgumentNullException(nameof(Message));
    }
}
