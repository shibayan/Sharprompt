using System;

using Sharprompt.Internal;

namespace Sharprompt.Forms
{
    internal abstract class FormBase<T> : IDisposable
    {
        protected FormBase(bool cursorVisible = true)
        {
            Scope = new ConsoleScope(cursorVisible);
        }

        protected ConsoleScope Scope { get; }

        public abstract T Start();

        public void Dispose()
        {
            Scope.Dispose();
        }
    }
}
