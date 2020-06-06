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
            var input = Scope.ReadLine();

            if (string.IsNullOrEmpty(input))
            {
                if (_defaultValue != null)
                {
                    result = _defaultValue.Value;

                    return true;
                }

                Scope.SetError(new ValidationError("Value is required"));
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

                Scope.SetError(new ValidationError("Value is invalid"));
            }

            result = default;

            return false;
        }

        protected override void InputTemplate(IConsoleRenderer consoleRenderer)
        {
            consoleRenderer.WriteMessage(_message);

            if (_defaultValue != null)
            {
                consoleRenderer.Write($"({(_defaultValue.Value ? "yes" : "no")}) ");
            }
            else
            {
                consoleRenderer.Write("(y/N) ");
            }
        }

        protected override void FinishTemplate(IConsoleRenderer consoleRenderer, bool result)
        {
            consoleRenderer.WriteMessage(_message);
            consoleRenderer.Write(result ? "Yes" : "No", Prompt.ColorSchema.Answer);
        }
    }
}
