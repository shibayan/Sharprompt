using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

using Sharprompt.Drivers;
using Sharprompt.Internal;

namespace Sharprompt.Forms
{
    internal abstract class FormBase<T> : IDisposable where T : notnull
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

        protected abstract bool TryGetResult([NotNullWhen(true)] out T? result);

        protected virtual void InputTemplate(ScreenBuffer screenBuffer)
        {
        }

        protected virtual void FinishTemplate(ScreenBuffer screenBuffer, T result)
        {
        }

        protected bool TryValidate(T? input, IReadOnlyList<Func<object?, ValidationResult>> validators)
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
