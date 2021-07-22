using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

using Sharprompt.Drivers;
using Sharprompt.Internal;

namespace Sharprompt.Forms
{
    internal abstract class FormBase<T> : IDisposable
    {
        protected FormBase(bool cursorVisible = true)
        {
            ConsoleDriver = new DefaultConsoleDriver();

            Renderer = new FormRenderer(ConsoleDriver, cursorVisible);
        }

        protected IConsoleDriver ConsoleDriver { get; }

        protected FormRenderer Renderer { get; }

        public void Dispose()
        {
            Renderer.Dispose();
        }

        public T Start(CancellationToken stoptoken)
        {
            while (true)
            {
                Renderer.Render(InputTemplate);

                if (!TryGetResult(stoptoken, out var result))
                {
                    if (!stoptoken.IsCancellationRequested)
                    {
                        continue;
                    }
                }

                if (!stoptoken.IsCancellationRequested)
                {
                    Renderer.Render(FinishTemplate, result);
                }

                return result;
            }
        }

        protected abstract bool TryGetResult(CancellationToken stoptoken, out T result);

        protected abstract void InputTemplate(OffscreenBuffer screenBuffer);

        protected abstract void FinishTemplate(OffscreenBuffer screenBuffer, T result);

        protected bool TryValidate(object input, IList<Func<object, ValidationResult>> validators)
        {
            foreach (var validator in validators)
            {
                var result = validator(input);

                if (result != ValidationResult.Success)
                {
                    Renderer.SetValidationResult(result);

                    return false;
                }
            }

            return true;
        }
    }
}
