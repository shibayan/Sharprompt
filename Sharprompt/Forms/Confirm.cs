using Sharprompt.Internal;
using Sharprompt.Validations;

namespace Sharprompt.Forms
{
    internal class Confirm : FormBase<bool>
    {
        public Confirm(string message, bool? defaultValue)
        {
            _message = message;
            _defaultValue = defaultValue;
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

        protected override void InputTemplate(ScreenBuffer screenBuffer)
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

        protected override void FinishTemplate(ScreenBuffer screenBuffer, bool result)
        {
            screenBuffer.WriteFinish(_message);
            screenBuffer.Write(result ? "Yes" : "No", Prompt.ColorSchema.Answer);
        }
    }
}
