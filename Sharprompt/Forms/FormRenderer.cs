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

            ConsoleDriver = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? new WindowsConsoleDriver() : new DefaultConsoleDriver();
        }

        private readonly bool _cursorVisible;

        private int _writtenLines;
        private int _writtenErrorLines;

        private string _errorMessage;

        public IConsoleDriver ConsoleDriver { get; }

        public void Dispose()
        {
            ConsoleDriver.Dispose();
        }

        public void Write(string value)
        {
            _writtenLines += ConsoleDriver.Write(value);
        }

        public void Write(string value, ConsoleColor color)
        {
            _writtenLines += ConsoleDriver.Write(value, color);
        }

        public void WriteMessage(string message)
        {
            _writtenLines += ConsoleDriver.Write(Symbol.Prompt, ConsoleColor.Green);
            _writtenLines += ConsoleDriver.Write($" {message}: ");
        }

        public void WriteFinishMessage(string message)
        {
            _writtenLines += ConsoleDriver.Write(Symbol.Done, ConsoleColor.Green);
            _writtenLines += ConsoleDriver.Write($" {message}: ");
        }

        public void WriteLine()
        {
            _writtenLines += ConsoleDriver.WriteLine();
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
            ConsoleDriver.CursorVisible = false;

            ClearAll();

            template(this);

            if (_errorMessage != null)
            {
                WriteErrorMessage(_errorMessage);

                _errorMessage = null;
            }

            ConsoleDriver.CursorVisible = _cursorVisible;
        }

        public void Render<TModel>(Action<FormRenderer, TModel> template, TModel result)
        {
            ConsoleDriver.CursorVisible = false;

            ClearAll();

            template(this, result);

            ConsoleDriver.CursorVisible = _cursorVisible;
        }

        private void ClearAll()
        {
            var top = ConsoleDriver.CursorTop;

            var bottom = top + _writtenErrorLines;

            for (int i = 0; i <= _writtenLines + _writtenErrorLines; i++)
            {
                ConsoleDriver.ClearLine(bottom - i);
            }

            _writtenLines = 0;
            _writtenErrorLines = 0;
        }

        private void WriteErrorMessage(string errorMessage)
        {
            var left = ConsoleDriver.CursorLeft;

            var writtenErrorLines = ConsoleDriver.WriteLine();

            writtenErrorLines += ConsoleDriver.Write($"{Symbol.Error} {errorMessage}", ConsoleColor.Red);

            ConsoleDriver.SetCursorPosition(left, ConsoleDriver.CursorTop - writtenErrorLines);

            _writtenErrorLines += writtenErrorLines;
        }
    }
}
