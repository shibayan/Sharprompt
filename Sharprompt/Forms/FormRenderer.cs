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

        private int _writtenLines;
        private int _writtenErrorLines;

        private string _errorMessage;

        public void Dispose()
        {
            _consoleDriver.CursorVisible = true;

            _consoleDriver.Dispose();
        }

        public void Beep()
        {
            _consoleDriver.Beep();
        }

        public ConsoleKeyInfo ReadKey() => _consoleDriver.ReadKey();

        public string ReadLine() => _consoleDriver.ReadLine();

        public void Write(string value)
        {
            _writtenLines += _consoleDriver.Write(value);
        }

        public void Write(string value, ConsoleColor color)
        {
            _writtenLines += _consoleDriver.Write(value, color);
        }

        public void WriteMessage(string message)
        {
            _writtenLines += _consoleDriver.Write("?", ConsoleColor.Green);
            _writtenLines += _consoleDriver.Write($" {message}: ");
        }

        public void WriteLine()
        {
            _writtenLines += _consoleDriver.WriteLine();
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

        public void Render(Action<FormRenderer> template)
        {
            _consoleDriver.CursorVisible = false;

            ClearAll();

            template(this);

            if (_errorMessage != null)
            {
                WriteErrorMessage(_errorMessage);

                _errorMessage = null;
            }

            _consoleDriver.CursorVisible = _cursorVisible;
        }

        public void Render<TModel>(Action<FormRenderer, TModel> template, TModel model)
        {
            _consoleDriver.CursorVisible = false;

            ClearAll();

            template(this, model);

            _consoleDriver.CursorVisible = _cursorVisible;
        }

        private void ClearAll()
        {
            var bottom = _consoleDriver.CursorTop + _writtenErrorLines;

            for (int i = 0; i <= _writtenLines + _writtenErrorLines; i++)
            {
                _consoleDriver.ClearLine(bottom - i);
            }

            _consoleDriver.SetCursorPosition(0, _consoleDriver.CursorTop);

            _writtenLines = 0;
            _writtenErrorLines = 0;
        }

        private void WriteErrorMessage(string errorMessage)
        {
            var left = _consoleDriver.CursorLeft;

            var writtenErrorLines = _consoleDriver.WriteLine();

            writtenErrorLines += _consoleDriver.Write($">> {errorMessage}", ConsoleColor.Red);

            _consoleDriver.SetCursorPosition(left, _consoleDriver.CursorTop - writtenErrorLines);

            _writtenErrorLines += writtenErrorLines;
        }
    }
}
