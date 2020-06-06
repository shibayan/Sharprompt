using Sharprompt.Drivers;
using Sharprompt.Internal;

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
            var input = Renderer.ReadLine();

            if (string.IsNullOrEmpty(input))
            {
                if (_defaultValue != null)
                {
                    result = _defaultValue.Value;

                    return true;
                }

                Renderer.SetError(new ValidationError("Value is required"));
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

                Renderer.SetError(new ValidationError("Value is invalid"));
            }

            result = default;

            return false;
        }

        protected override void InputTemplate(IConsoleDriver consoleDriver)
        {
            consoleDriver.WriteMessage(_message);

            if (_defaultValue != null)
            {
                consoleDriver.Write($"({(_defaultValue.Value ? "yes" : "no")}) ");
            }
            else
            {
                consoleDriver.Write("(y/N) ");
            }
        }

        protected override void FinishTemplate(IConsoleDriver consoleDriver, bool result)
        {
            consoleDriver.WriteMessage(_message);
            consoleDriver.Write(result ? "Yes" : "No", Prompt.ColorSchema.Answer);
        }
    }
}
