using System;

namespace Sharprompt
{
    internal class Confirm
    {
        public Confirm(string message, bool? defaultValue)
        {
            _message = message;
            _defaultValue = defaultValue;
        }

        private readonly string _message;
        private readonly bool? _defaultValue;

        public bool Start()
        {
            using (var scope = new ConsoleScope())
            {
                bool result;

                while (true)
                {
                    scope.Render(Template, new TemplateModel { Message = _message, DefaultValue = _defaultValue });

                    var input = scope.ReadLine();

                    if (string.IsNullOrEmpty(input))
                    {
                        if (_defaultValue != null)
                        {
                            result = _defaultValue.Value;
                            break;
                        }

                        scope.SetError(new ValidationError("Value is required"));
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

                        scope.SetError(new ValidationError("Value is invalid"));
                    }
                }

                scope.Render(FinishTemplate, new FinishTemplateModel { Message = _message, Result = result });

                return result;
            }
        }

        private void Template(ConsoleRenderer renderer, TemplateModel model)
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

        private void FinishTemplate(ConsoleRenderer renderer, FinishTemplateModel model)
        {
            renderer.WriteMessage(model.Message);
            renderer.Write(model.Result ? "Yes" : "No", ConsoleColor.Cyan);
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
