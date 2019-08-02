using System;
using System.Collections.Generic;

namespace Sharprompt
{
    internal class Input<T>
    {
        public Input(string message, object defaultValue = null, IList<Func<object, ValidationError>> validators = null)
        {
            _message = message;
            _defaultValue = defaultValue;
            _validators = validators;
            _targetType = typeof(T);
        }

        private readonly string _message;
        private readonly object _defaultValue;
        private readonly IList<Func<object, ValidationError>> _validators;
        private readonly Type _targetType;

        private T _result;

        public T Start()
        {
            using (var scope = new ConsoleScope())
            {
                while (true)
                {
                    scope.Render(Template);

                    var input = scope.ReadLine();

                    if (!scope.Validate(input, _validators))
                    {
                        continue;
                    }

                    if (string.IsNullOrEmpty(input))
                    {
                        _result = (T)_defaultValue;
                        break;
                    }

                    try
                    {
                        _result = (T)Convert.ChangeType(input, Nullable.GetUnderlyingType(_targetType) ?? _targetType);
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
