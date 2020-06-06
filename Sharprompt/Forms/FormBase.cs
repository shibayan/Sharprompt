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

        public void Dispose()
        {
            Scope.Dispose();
        }

        public T Start()
        {
            while (true)
            {
                Scope.Render(InputTemplate);

                if (!TryGetResult(out var result))
                {
                    continue;
                }

                Scope.Render(FinishTemplate, result);

                return result;
            }
        }

        protected abstract bool TryGetResult(out T result);

        protected virtual void InputTemplate(IConsoleRenderer consoleRenderer)
        {
        }

        protected virtual void FinishTemplate(IConsoleRenderer consoleRenderer, T result)
        {
        }
    }
}
