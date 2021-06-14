using System.ComponentModel.DataAnnotations;

using Sharprompt.Internal;
using Sharprompt.Models;

namespace Sharprompt.Forms
{
    internal class Confirm : FormBase<bool>
    {
        public Confirm(ConfirmOptions options)
        {
            _message = options.Message;
            _defaultValue = options.DefaultValue;
        }

        private readonly string _message;
        private readonly bool? _defaultValue;

        protected override bool TryGetResult(out bool result)
        {
            var input = ConsoleDriver.ReadLine();

            if (string.IsNullOrEmpty(input))
            {
                if (_defaultValue != null)
                {
                    result = _defaultValue.Value;

                    return true;
                }
            }
            else
            {
                var lowerInput = input.ToLower();

                if (lowerInput == "y" || lowerInput == "yes")
                {
                    result = true;

                    return true;
                }

                if (lowerInput == "n" || lowerInput == "no")
                {
                    result = false;

                    return true;
                }
            }

            Renderer.SetValidationResult(new ValidationResult("Value is invalid"));

            result = default;

            return false;
        }

        protected override void InputTemplate(OffscreenBuffer screenBuffer)
        {
            screenBuffer.WritePrompt(_message);

            if (_defaultValue == null)
            {
                screenBuffer.Write("(y/n) ");
            }
            else if (_defaultValue.Value)
            {
                screenBuffer.Write("(Y/n) ");
            }
            else
            {
                screenBuffer.Write("(y/N) ");
            }

            screenBuffer.SetCursorPosition();
        }

        protected override void FinishTemplate(OffscreenBuffer screenBuffer, bool result)
        {
            screenBuffer.WriteFinish(_message);
            screenBuffer.Write(result ? "Yes" : "No", Prompt.ColorSchema.Answer);
        }
    }
}
