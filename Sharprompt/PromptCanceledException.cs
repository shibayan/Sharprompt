using System;
using System.Runtime.Serialization;

namespace Sharprompt;

[Serializable]
public class PromptCanceledException : Exception
{
    public PromptCanceledException()
    {
    }

    public PromptCanceledException(string message)
        : base(message)
    {
    }

    public PromptCanceledException(string message, Exception inner)
        : base(message, inner)
    {
    }

    public PromptCanceledException(string message, string promptType)
        : base(message)
    {
        PromptType = promptType;
    }

    protected PromptCanceledException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }

    public string PromptType { get; }
}
