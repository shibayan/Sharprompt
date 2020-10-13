using System;
using System.Collections.Generic;

using Sharprompt.Drivers;
using Sharprompt.Validations;

namespace Sharprompt.Forms
{
    internal abstract class FormBase<T> : IDisposable
    {
        protected FormBase(bool cursorVisible = true)
        {
            Renderer = new FormRenderer(cursorVisible);
        }

        protected FormRenderer Renderer { get; }

        protected IConsoleDriver ConsoleDriver => Renderer.ConsoleDriver;

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

        protected virtual void InputTemplate(FormRenderer formRenderer)
        {
        }

        protected virtual void FinishTemplate(FormRenderer formRenderer, T result)
        {
        }

        protected bool TryValidate(object input, IList<Func<object, ValidationResult>> validators)
        {
            if (validators == null)
            {
                return true;
            }

            foreach (var validator in validators)
            {
                var result = validator(input);

                if (result != null)
                {
                    Renderer.SetValidationResult(result);

                    return false;
                }
            }

            return true;
        }
    }
}
