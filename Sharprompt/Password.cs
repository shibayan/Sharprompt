using System;
using System.Collections.Generic;
using System.Text;

namespace Sharprompt
{
    public class Password
    {
        public Password(string message, IList<Func<object, Error>> validators = null)
        {
            _message = message;
            _validators = validators;
        }

        private readonly string _message;
        private readonly IList<Func<object, Error>> _validators;

        private StringBuilder _buffer;

        public string Start()
        {
            using (var scope = new ConsoleScope())
            {
                _buffer = new StringBuilder(64);

                while (true)
                {
                    scope.Render(Template);

                    var keyInfo = scope.ReadKey();

                    if (keyInfo.Key == ConsoleKey.Enter)
                    {
                        if (scope.Validate(_buffer.ToString(), _validators))
                        {
                            break;
                        }

                        _buffer.Clear();
                    }
                    else if (keyInfo.Key == ConsoleKey.Backspace)
                    {
                        if (_buffer.Length == 0)
                        {
                            scope.Beep();
                        }
                        else
                        {
                            _buffer.Length -= 1;
                        }
                    }
                    else if (!char.IsControl(keyInfo.KeyChar))
                    {
                        _buffer.Append(keyInfo.KeyChar);
                    }
                }

                return _buffer.ToString();
            }
        }

        private void Template(ConsoleRenderer renderer)
        {
            renderer.WriteMessage(_message);

            renderer.Write(new string('*', _buffer.Length));
        }
    }
}
