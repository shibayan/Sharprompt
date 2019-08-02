using System;

namespace Sharprompt
{
    internal class Confirm
    {
        public Confirm(string message, bool? defaultValue = null)
        {
            _message = message;
            _defaultValue = defaultValue;
        }

        private readonly string _message;
        private readonly bool? _defaultValue;

        private bool _result;

        public bool Start()
        {
            using (var scope = new ConsoleScope())
            {
                while (true)
                {
                    scope.Render(Template);

                    var input = scope.ReadLine();

                    if (string.IsNullOrEmpty(input))
                    {
                        if (_defaultValue != null)
                        {
                            _result = _defaultValue.Value;
                            break;
                        }

                        scope.SetError(new ValidationError("Value is required"));
                    }
                    else
                    {
                        var lowerInput = input.ToLower();

                        if (lowerInput == "y" || lowerInput == "yes")
                        {
                            _result = true;
                            break;
                        }

                        if (lowerInput == "n" || lowerInput == "no")
                        {
                            _result = false;
                            break;
                        }

                        scope.SetError(new ValidationError("Value is invalid"));
                    }
                }

                scope.Render(FinishTemplate);

                return _result;
            }
        }

        private void Template(ConsoleRenderer renderer)
        {
            renderer.WriteMessage(_message);

            if (_defaultValue != null)
            {
                renderer.Write($"({(_defaultValue.Value ? "yes" : "no")}) ");
            }
            else
            {
                renderer.Write("(y/N) ");
            }
        }

        private void FinishTemplate(ConsoleRenderer renderer)
        {
            renderer.WriteMessage(_message);
            renderer.Write(_result ? "Yes" : "No", ConsoleColor.Cyan);
        }
    }
}
