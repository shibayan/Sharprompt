using System;

namespace Sharprompt;

/// <summary>
/// Indicates that the user canceled a prompt with Ctrl+C or Escape while ThrowExceptionOnCancel was enabled. Token cancellation throws OperationCanceledException instead.
/// </summary>
public class PromptCanceledException : Exception
{
    /// <summary>
    /// Creates a prompt cancellation exception.
    /// </summary>
    public PromptCanceledException()
    {
    }

    /// <summary>
    /// Creates a prompt cancellation exception.
    /// </summary>
    /// <param name="message">The cancellation message.</param>
    public PromptCanceledException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Creates a prompt cancellation exception.
    /// </summary>
    /// <param name="message">The cancellation message.</param>
    /// <param name="inner">The underlying exception.</param>
    public PromptCanceledException(string message, Exception inner)
        : base(message, inner)
    {
    }

    /// <summary>
    /// Creates a prompt cancellation exception.
    /// </summary>
    /// <param name="message">The cancellation message.</param>
    /// <param name="promptType">The name of the form that was canceled.</param>
    public PromptCanceledException(string message, string promptType)
        : base(message)
    {
        PromptType = promptType;
    }

    /// <summary>
    /// The form name supplied by the canceled prompt, or null when no name was supplied.
    /// </summary>
    public string? PromptType { get; }
}
