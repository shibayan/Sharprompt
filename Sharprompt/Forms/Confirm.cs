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

        public override bool Start()
        {
            bool result;

            while (true)
            {
                Scope.Render(Template, new TemplateModel { Message = _message, DefaultValue = _defaultValue });

                var input = Scope.ReadLine();

                if (string.IsNullOrEmpty(input))
                {
                    if (_defaultValue != null)
                    {
                        result = _defaultValue.Value;
                        break;
                    }

                    Scope.SetError(new ValidationError("Value is required"));
                }
                else
                {
                    var lowerInput = input.ToLower();

                    if (lowerInput == "y" || lowerInput == "yes")
                    {
                        result = true;
                        break;
                    }

                    if (lowerInput == "n" || lowerInput == "no")
                    {
                        result = false;
                        break;
                    }

                    Scope.SetError(new ValidationError("Value is invalid"));
                }
            }

            Scope.Render(FinishTemplate, new FinishTemplateModel { Message = _message, Result = result });

            return result;
        }

        private void Template(IConsoleRenderer renderer, TemplateModel model)
        {
            renderer.WriteMessage(model.Message);

            if (model.DefaultValue != null)
            {
                renderer.Write($"({(model.DefaultValue.Value ? "yes" : "no")}) ");
            }
            else
            {
                renderer.Write("(y/N) ");
            }
        }

        private void FinishTemplate(IConsoleRenderer renderer, FinishTemplateModel model)
        {
            renderer.WriteMessage(model.Message);
            renderer.Write(model.Result ? "Yes" : "No", Prompt.ColorSchema.Answer);
        }

        private class TemplateModel
        {
            public string Message { get; set; }
            public bool? DefaultValue { get; set; }
        }

        private class FinishTemplateModel
        {
            public string Message { get; set; }
            public bool Result { get; set; }
        }
    }
}
