using System;
using System.Collections.Generic;

using Sharprompt.Internal;

namespace Sharprompt.Forms
{
    internal class Input<T> : FormBase<T>
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

        public override T Start()
        {
            T result;

            while (true)
            {
                Scope.Render(Template, new TemplateModel { Message = _message, DefaultValue = _defaultValue });

                var input = Scope.ReadLine();

                if (!Scope.Validate(input, _validators))
                {
                    continue;
                }

                if (string.IsNullOrEmpty(input))
                {
                    if (_targetType.IsValueType && _underlyingType == null && _defaultValue == null)
                    {
                        Scope.SetError(new ValidationError("Value is required"));

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
                    Scope.SetException(ex);
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
                renderer.Write($"({model.DefaultValue}) ");
            }
        }

        private void FinishTemplate(IConsoleRenderer renderer, FinishTemplateModel model)
        {
            renderer.WriteMessage(model.Message);

            if (model.Result != null)
            {
                renderer.Write(model.Result.ToString(), Prompt.ColorSchema.Answer);
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
