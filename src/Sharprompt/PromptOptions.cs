using System;

namespace Sharprompt;

/// <summary>
/// Common settings for an interactive prompt.
/// </summary>
public abstract class PromptOptions
{
    /// <summary>
    /// The prompt message. Must be set before starting the prompt.
    /// </summary>
    public string Message { get; set; } = null!;

    /// <summary>
    /// Checks that the message required by every prompt has been supplied.
    /// </summary>
    /// <exception cref="ArgumentNullException">A required setting is null.</exception>
    internal virtual void EnsureOptions()
    {
        ArgumentNullException.ThrowIfNull(Message);
    }
}
