using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Sharprompt.Drivers;

namespace Sharprompt.Forms
{
    internal class FormRenderer : IDisposable
    {
        public FormRenderer(bool cursorVisible = true)
        {
            _cursorVisible = cursorVisible;

            _consoleDriver = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? new WindowsConsoleDriver() : new DefaultConsoleDriver();
        }

        private readonly bool _cursorVisible;
        private readonly IConsoleDriver _consoleDriver;

        private string _errorMessage;

        public void Dispose()
        {
            _consoleDriver.Close();

            Console.CursorVisible = true;
        }

        public void Beep()
        {
            Console.Beep();
        }

        public ConsoleKeyInfo ReadKey()
        {
            return Console.ReadKey(true);
        }

        public string ReadLine()
        {
            var left = Console.CursorLeft;

            var line = Console.ReadLine();

            if (line != null)
            {
                Console.SetCursorPosition(left, Console.CursorTop - 1);
            }

            return line;
        }

        public void SetError(ValidationError error)
        {
            _errorMessage = error.Message;
        }

        public bool Validate(object input, IList<Func<object, ValidationError>> validators)
        {
            if (validators == null)
            {
                return true;
            }

            foreach (var validator in validators)
            {
                var error = validator(input);

                if (error != null)
                {
                    _errorMessage = error.Message;

                    return false;
                }
            }

            return true;
        }

        public void SetException(Exception exception)
        {
            _errorMessage = exception.Message;
        }

        public void Render(Action<IConsoleDriver> template)
        {
            Console.CursorVisible = false;

            _consoleDriver.Reset();

            template(_consoleDriver);

            if (_errorMessage != null)
            {
                _consoleDriver.WriteErrorMessage(_errorMessage);

                _errorMessage = null;
            }

            Console.CursorVisible = _cursorVisible;
        }

        public void Render<TModel>(Action<IConsoleDriver, TModel> template, TModel model)
        {
            Console.CursorVisible = false;

            _consoleDriver.Reset();

            template(_consoleDriver, model);

            Console.CursorVisible = _cursorVisible;
        }
    }
}
