using System;
using System.ComponentModel.DataAnnotations;

using Sharprompt.Drivers;
using Sharprompt.Internal;

namespace Sharprompt.Forms
{
    internal class FormRenderer : IDisposable
    {
        public FormRenderer(bool cursorVisible = true)
        {
            ConsoleDriver = new DefaultConsoleDriver();

            _cursorVisible = cursorVisible;
            _screenBuffer = new ScreenBuffer(ConsoleDriver);
        }

        private readonly bool _cursorVisible;
        private readonly ScreenBuffer _screenBuffer;

        private string _errorMessage;

        public IConsoleDriver ConsoleDriver { get; }

        public void Dispose()
        {
            ConsoleDriver.Dispose();
        }

        public void SetValidationResult(ValidationResult result)
        {
            _errorMessage = result.ErrorMessage;
        }

        public void SetException(Exception exception)
        {
            _errorMessage = exception.Message;
        }

        public void Render(Action<ScreenBuffer> template)
        {
            ConsoleDriver.CursorVisible = false;

            ClearAll();

            template(_screenBuffer);

            if (_errorMessage != null)
            {
                _screenBuffer.WriteErrorMessage(_errorMessage);

                _errorMessage = null;
            }

            _screenBuffer.RenderToConsole();

            ConsoleDriver.CursorVisible = _cursorVisible;
        }

        public void Render<TModel>(Action<ScreenBuffer, TModel> template, TModel result)
        {
            ConsoleDriver.CursorVisible = false;

            ClearAll();

            template(_screenBuffer, result);

            _screenBuffer.RenderToConsole();

            ConsoleDriver.WriteLine();

            ConsoleDriver.CursorVisible = _cursorVisible;
        }

        private void ClearAll()
        {
            var bottom = _screenBuffer.CursorBottom;

            for (var i = 0; i < _screenBuffer.LineCount; i++)
            {
                ConsoleDriver.ClearLine(bottom - i);
            }

            _screenBuffer.Clear();
        }
    }
}
