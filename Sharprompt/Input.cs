using System;
using System.Collections.Generic;

namespace Sharprompt
{
    public class Input<T>
    {
        public Input(string message, object defaultValue = null, IList<Func<object, Error>> validators = null)
        {
            _message = message;
            _defaultValue = defaultValue;
            _validators = validators;
        }

        private readonly string _message;
        private readonly object _defaultValue;
        private readonly IList<Func<object, Error>> _validators;

        private T _result;

        public T Start()
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
                            _result = (T)_defaultValue;
                            break;
                        }
                    }

                    if (!scope.Validate(input, _validators))
                    {
                        continue;
                    }

                    try
                    {
                        _result = (T)Convert.ChangeType(input, typeof(T));
                        break;
                    }
                    catch (Exception ex)
                    {
                        scope.SetException(ex);
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
