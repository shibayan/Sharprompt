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
        }

        private readonly string _message;
        private readonly object _defaultValue;
        private readonly IList<Func<object, ValidationError>> _validators;
        private readonly Type _targetType;

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
                        result = (T)_defaultValue;
                        break;
                    }

                    try
                    {
                        result = (T)Convert.ChangeType(input, Nullable.GetUnderlyingType(_targetType) ?? _targetType);
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
            renderer.Write(model.Result.ToString(), ConsoleColor.Cyan);
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
