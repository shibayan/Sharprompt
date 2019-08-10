using System;
using System.Collections.Generic;

namespace Sharprompt
{
    internal class Input<T>
    {
        public Input(string message, object defaultValue, IList<Func<object, ValidationError>> validators)
        {
            _message = message;
            _defaultValue = defaultValue;
            _validators = validators;
            _targetType = typeof(T);
            _underlyingType = Nullable.GetUnderlyingType(_targetType);
        }

        private readonly string _message;
        private readonly object _defaultValue;
        private readonly IList<Func<object, ValidationError>> _validators;
        private readonly Type _targetType;
        private readonly Type _underlyingType;

        public T Start()
        {
            using (var scope = new ConsoleScope())
            {
                T result;

                while (true)
                {
                    scope.Render(Template, new TemplateModel { Message = _message, DefaultValue = _defaultValue });

                    var input = scope.ReadLine();

                    if (!scope.Validate(input, _validators))
                    {
                        continue;
                    }

                    if (string.IsNullOrEmpty(input))
                    {
                        if (_targetType.IsValueType && _underlyingType == null && _defaultValue == null)
                        {
                            scope.SetError(new ValidationError("Value is required"));

                            continue;
                        }

                        result = (T)_defaultValue;

                        break;
                    }

                    try
                    {
                        result = (T)Convert.ChangeType(input, _underlyingType ?? _targetType);
                        break;
                    }
                    catch (Exception ex)
                    {
                        scope.SetException(ex);
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
                renderer.Write($"({model.DefaultValue}) ");
            }
        }

        private void FinishTemplate(ConsoleRenderer renderer, FinishTemplateModel model)
        {
            renderer.WriteMessage(model.Message);

            if (model.Result != null)
            {
                renderer.Write(model.Result.ToString(), ConsoleColor.Cyan);
            }
        }

        private class TemplateModel
        {
            public string Message { get; set; }
            public object DefaultValue { get; set; }
        }

        private class FinishTemplateModel
        {
            public string Message { get; set; }
            public T Result { get; set; }
        }
    }
}
