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

            _formRenderer = new FormRenderer(ConsoleDriver, cursorVisible);
        }

        private readonly FormRenderer _formRenderer;

        protected IConsoleDriver ConsoleDriver { get; }

        public void Dispose()
        {
            _formRenderer.Dispose();
        }

        public T Start()
        {
            while (true)
            {
                _formRenderer.Render(InputTemplate);

                if (!TryGetResult(out var result))
                {
                    continue;
                }

                _formRenderer.Render(FinishTemplate, result);

                return result;
            }
        }

        protected abstract bool TryGetResult(out T result);

        protected abstract void InputTemplate(OffscreenBuffer screenBuffer);

        protected abstract void FinishTemplate(OffscreenBuffer screenBuffer, T result);

        protected void SetValidationResult(ValidationResult validationResult)
        {
            _formRenderer.ErrorMessage = validationResult.ErrorMessage;
        }

        protected void SetException(Exception exception)
        {
            _formRenderer.ErrorMessage = exception.Message;
        }

        protected bool TryValidate(object input, IList<Func<object, ValidationResult>> validators)
        {
            foreach (var validator in validators)
            {
                var result = validator(input);

                if (result != ValidationResult.Success)
                {
                    SetValidationResult(result);

                    return false;
                }
            }

            return true;
        }
    }
}
