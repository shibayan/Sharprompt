using System;
using System.Runtime.InteropServices;

using Sharprompt.Drivers;
using Sharprompt.Internal;
using Sharprompt.Validations;

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
            _writtenLines += _consoleDriver.Write(Symbol.Prompt, ConsoleColor.Green);
            _writtenLines += _consoleDriver.Write($" {message}: ");
        }

        public void WriteFinishMessage(string message)
        {
            _writtenLines += _consoleDriver.Write(Symbol.Done, ConsoleColor.Green);
            _writtenLines += _consoleDriver.Write($" {message}: ");
        }

        public void WriteLine()
        {
            _writtenLines += _consoleDriver.WriteLine();
        }

        public void SetValidationResult(ValidationResult result)
        {
            _errorMessage = result.ErrorMessage;
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

        public void Render<TModel>(Action<FormRenderer, TModel> template, TModel result)
        {
            _consoleDriver.CursorVisible = false;

            ClearAll();

            template(this, result);

            _consoleDriver.CursorVisible = _cursorVisible;
        }

        private void ClearAll()
        {
            var (_, top) = _consoleDriver.GetCursorPosition();

            var bottom = top + _writtenErrorLines;

            for (int i = 0; i <= _writtenLines + _writtenErrorLines; i++)
            {
                _consoleDriver.ClearLine(bottom - i);
            }

            _writtenLines = 0;
            _writtenErrorLines = 0;
        }

        private void WriteErrorMessage(string errorMessage)
        {
            var (left, _) = _consoleDriver.GetCursorPosition();

            var writtenErrorLines = _consoleDriver.WriteLine();

            writtenErrorLines += _consoleDriver.Write($"{Symbol.Error} {errorMessage}", ConsoleColor.Red);

            _consoleDriver.SetCursorPosition(left, _consoleDriver.CursorTop - writtenErrorLines);

            _writtenErrorLines += writtenErrorLines;
        }
    }
}
