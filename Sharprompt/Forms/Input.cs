using System;
using System.Collections.Generic;

namespace Sharprompt.Forms
{
    internal class Input<T> : FormBase<T>
    {
        public Input(string message, object defaultValue, IList<Func<object, ValidationError>> validators)
        {
            _message = message;
            _defaultValue = defaultValue;
            _validators = validators;
        }

        private readonly string _message;
        private readonly object _defaultValue;
        private readonly IList<Func<object, ValidationError>> _validators;

        private readonly Type _targetType = typeof(T);
        private readonly Type _underlyingType = Nullable.GetUnderlyingType(typeof(T));

        protected override bool TryGetResult(out T result)
        {
            var input = Renderer.ReadLine();

            if (!Renderer.Validate(input, _validators))
            {
                result = default;

                return false;
            }

            if (string.IsNullOrEmpty(input))
            {
                if (_targetType.IsValueType && _underlyingType == null && _defaultValue == null)
                {
                    Renderer.SetError(new ValidationError("Value is required"));

                    result = default;

                    return false;
                }

                result = (T)_defaultValue;

                return true;
            }

            try
            {
                result = (T)Convert.ChangeType(input, _underlyingType ?? _targetType);

                return true;
            }
            catch (Exception ex)
            {
                Renderer.SetException(ex);
            }

            result = default;

            return false;
        }

        protected override void InputTemplate(FormRenderer formRenderer)
        {
            formRenderer.WriteMessage(_message);

            if (_defaultValue != null)
            {
                formRenderer.Write($"({_defaultValue}) ");
            }
        }

        protected override void FinishTemplate(FormRenderer formRenderer, T result)
        {
            formRenderer.WriteMessage(_message);

            if (result != null)
            {
                formRenderer.Write(result.ToString(), Prompt.ColorSchema.Answer);
            }
        }
    }
}
