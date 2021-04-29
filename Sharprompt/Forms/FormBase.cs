using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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

        public T Start()
        {
            while (true)
            {
                Renderer.Render(InputTemplate);

                if (!TryGetResult(out var result))
                {
                    continue;
                }

                Renderer.Render(FinishTemplate, result);

                return result;
            }
        }

        protected abstract bool TryGetResult(out T result);

        protected abstract void InputTemplate(OffscreenBuffer screenBuffer);

        protected abstract void FinishTemplate(OffscreenBuffer screenBuffer, T result);

        protected bool TryValidate(object input, IList<Func<object, ValidationResult>> validators)
        {
            if (validators == null)
            {
                return true;
            }

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
