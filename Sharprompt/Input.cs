using System;
using System.Collections.Generic;

namespace Sharprompt
{
    public class Input<T>
    {
        public Input(string message, object defaultValue = null, IList<Func<object, bool>> validators = null)
        {
            _message = message;
            _defaultValue = defaultValue;
        }

        private readonly string _message;
        private readonly object _defaultValue;

        private T _result;

        public T Start()
        {
            using (var scope = new ConsoleScope(true))
            {
                while (true)
                {
                    scope.Render(Template);

                    var input = scope.ReadLine();

                    if (string.IsNullOrWhiteSpace(input))
                    {
                        if (_defaultValue != null)
                        {
                            _result = (T)_defaultValue;
                            break;
                        }

                        scope.SetError("Value is required");
                    }
                    else
                    {
                        try
                        {
                            _result = (T)Convert.ChangeType(input, typeof(T));
                            break;
                        }
                        catch (Exception ex)
                        {
                            scope.SetError(ex);
                        }
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
                renderer.Write($"({_defaultValue}) ");
            }
        }

        private void FinishTemplate(ConsoleRenderer renderer)
        {
            renderer.WriteMessage(_message);
            renderer.Write(_result.ToString(), ConsoleColor.Cyan);
        }
    }
}
