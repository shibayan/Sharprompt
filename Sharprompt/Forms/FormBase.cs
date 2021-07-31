using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Sharprompt.Drivers;
using Sharprompt.Internal;

namespace Sharprompt.Forms
{
    internal abstract class FormBase<T> : IDisposable
    {
        protected FormBase()
        {
            ConsoleDriver = new DefaultConsoleDriver();

            _formRenderer = new FormRenderer(ConsoleDriver);
        }

        private readonly FormRenderer _formRenderer;

        protected IConsoleDriver ConsoleDriver { get; }

        public void Dispose() => _formRenderer.Dispose();

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

        protected abstract void InputTemplate(OffscreenBuffer offscreenBuffer);

        protected abstract void FinishTemplate(OffscreenBuffer offscreenBuffer, T result);

        protected void SetError(string errorMessage) => _formRenderer.ErrorMessage = errorMessage;

        protected void SetError(ValidationResult validationResult) => _formRenderer.ErrorMessage = validationResult.ErrorMessage;

        protected void SetError(Exception exception) => _formRenderer.ErrorMessage = exception.Message;

        protected bool TryValidate(object input, IList<Func<object, ValidationResult>> validators)
        {
            foreach (var validator in validators)
            {
                var result = validator(input);

                if (result != ValidationResult.Success)
                {
                    SetError(result);

                    return false;
                }
            }

            return true;
        }
    }
}
