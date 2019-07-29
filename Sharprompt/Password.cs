using System;
using System.Text;

namespace Sharprompt
{
    internal class Password
    {
        public Password(string message)
        {
            _message = message;
        }

        private readonly string _message;

        private StringBuilder _buffer;

        public string Start()
        {
            using (var scope = new ConsoleScope(true))
            {
                _buffer = new StringBuilder();

                while (true)
                {
                    scope.Render(Template);

                    var keyInfo = scope.ReadKey();

                    if (keyInfo.Key == ConsoleKey.Enter)
                    {
                        break;
                    }

                    if (keyInfo.Key == ConsoleKey.Backspace)
                    {
                        if (_buffer.Length == 0)
                        {
                            scope.Beep();
                        }
                        else
                        {
                            _buffer.Length -= 1;
                        }

                        continue;
                    }

                    if (!char.IsControl(keyInfo.KeyChar))
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
