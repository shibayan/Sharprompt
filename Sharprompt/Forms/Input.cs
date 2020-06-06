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

        protected override bool TryGetResult(out T result)
        {
            var input = Scope.ReadLine();

            if (!Scope.Validate(input, _validators))
            {
                result = default;

                return false;
            }

            if (string.IsNullOrEmpty(input))
            {
                if (_targetType.IsValueType && _underlyingType == null && _defaultValue == null)
                {
                    Scope.SetError(new ValidationError("Value is required"));

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
                Scope.SetException(ex);
            }

            result = default;

            return false;
        }

        protected override void InputTemplate(IConsoleRenderer consoleRenderer)
        {
            consoleRenderer.WriteMessage(_message);

            if (_defaultValue != null)
            {
                consoleRenderer.Write($"({_defaultValue}) ");
            }
        }

        protected override void FinishTemplate(IConsoleRenderer consoleRenderer, T result)
        {
            consoleRenderer.WriteMessage(_message);

            if (result != null)
            {
                consoleRenderer.Write(result.ToString(), Prompt.ColorSchema.Answer);
            }
        }
    }
}
