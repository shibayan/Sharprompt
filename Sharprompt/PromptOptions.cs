using System;

namespace Sharprompt;

public abstract class PromptOptions
{
    public string Message { get; set; } = null!;

    internal virtual void EnsureOptions()
    {
        ArgumentNullException.ThrowIfNull(Message);
    }
}
