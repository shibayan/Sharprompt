using System;
using System.Runtime.Serialization;

namespace Sharprompt
{
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

        protected PromptCanceledException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public string PromptType { get; }
    }
}
