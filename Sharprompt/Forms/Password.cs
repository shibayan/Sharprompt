using System;
using System.Collections.Generic;

using Sharprompt.Internal;

namespace Sharprompt.Forms
{
    internal class Password : FormBase<string>
    {
        public Password(string message, IList<Func<object, ValidationError>> validators)
        {
            _message = message;
            _validators = validators;
        }

        private readonly string _message;
        private readonly IList<Func<object, ValidationError>> _validators;

        public override string Start()
        {
            var result = "";

            while (true)
            {
                Scope.Render(Template, new TemplateModel { Message = _message, InputLength = result.Length });

                var keyInfo = Scope.ReadKey();

                if (keyInfo.Key == ConsoleKey.Enter)
                {
                    if (Scope.Validate(result, _validators))
                    {
                        break;
                    }

                    result = "";
                }
                else if (keyInfo.Key == ConsoleKey.Backspace)
                {
                    if (result.Length == 0)
                    {
                        Scope.Beep();
                    }
                    else
                    {
                        result = result.Remove(result.Length - 1, 1);
                    }
                }
                else if (!char.IsControl(keyInfo.KeyChar))
                {
                    result += keyInfo.KeyChar;
                }
            }

            return result;
        }

        private void Template(IConsoleRenderer renderer, TemplateModel model)
        {
            renderer.WriteMessage(model.Message);
            renderer.Write(new string('*', model.InputLength));
        }

        private class TemplateModel
        {
            public string Message { get; set; }
            public int InputLength { get; set; }
        }
    }
}
