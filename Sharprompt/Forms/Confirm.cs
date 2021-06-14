using System.ComponentModel.DataAnnotations;

using Sharprompt.Internal;

namespace Sharprompt.Forms
{
    internal class Confirm : FormBase<bool>
    {
        public Confirm(ConfirmOptions options)
        {
            _options = options;
        }

        private readonly ConfirmOptions _options;

        protected override bool TryGetResult(out bool result)
        {
            var input = ConsoleDriver.ReadLine();

            if (string.IsNullOrEmpty(input))
            {
                if (_options.DefaultValue != null)
                {
                    result = _options.DefaultValue.Value;

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
            screenBuffer.WritePrompt(_options.Message);

            if (_options.DefaultValue == null)
            {
                screenBuffer.Write("(y/n) ");
            }
            else if (_options.DefaultValue.Value)
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
            screenBuffer.WriteFinish(_options.Message);
            screenBuffer.Write(result ? "Yes" : "No", Prompt.ColorSchema.Answer);
        }
    }
}
