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

                Renderer.SetValidationResult(new ValidationResult("Value is required"));
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

                Renderer.SetValidationResult(new ValidationResult("Value is invalid"));
            }

            result = default;

            return false;
        }

        protected override void InputTemplate(FormRenderer formRenderer)
        {
            formRenderer.WriteMessage(_message);

            if (_defaultValue != null)
            {
                formRenderer.Write($"({(_defaultValue.Value ? "yes" : "no")}) ");
            }
            else
            {
                formRenderer.Write("(y/N) ");
            }
        }

        protected override void FinishTemplate(FormRenderer formRenderer, bool result)
        {
            formRenderer.WriteFinishMessage(_message);
            formRenderer.Write(result ? "Yes" : "No", Prompt.ColorSchema.Answer);
        }
    }
}
