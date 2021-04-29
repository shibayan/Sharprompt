using System;
using System.ComponentModel.DataAnnotations;

using Sharprompt.Drivers;
using Sharprompt.Internal;

namespace Sharprompt.Forms
{
    internal class FormRenderer : IDisposable
    {
        public FormRenderer(IConsoleDriver consoleDriver, bool cursorVisible = true)
        {
            _consoleDriver = consoleDriver;
            _cursorVisible = cursorVisible;

            _offscreenBuffer = new OffscreenBuffer(_consoleDriver);
        }

        private readonly bool _cursorVisible;
        private readonly IConsoleDriver _consoleDriver;
        private readonly OffscreenBuffer _offscreenBuffer;

        private string _errorMessage;

        public void Dispose()
        {
            _consoleDriver.Dispose();
        }

        public void SetValidationResult(ValidationResult result)
        {
            _errorMessage = result.ErrorMessage;
        }

        public void SetException(Exception exception)
        {
            _errorMessage = exception.Message;
        }

        public void Render(Action<OffscreenBuffer> template)
        {
            _consoleDriver.CursorVisible = false;

            ClearAll();

            template(_offscreenBuffer);

            if (_errorMessage != null)
            {
                _offscreenBuffer.WriteErrorMessage(_errorMessage);

                _errorMessage = null;
            }

            _offscreenBuffer.RenderToConsole();

            _consoleDriver.CursorVisible = _cursorVisible;
        }

        public void Render<TModel>(Action<OffscreenBuffer, TModel> template, TModel result)
        {
            _consoleDriver.CursorVisible = false;

            ClearAll();

            template(_offscreenBuffer, result);

            _offscreenBuffer.RenderToConsole();

            _consoleDriver.WriteLine();

            _consoleDriver.CursorVisible = _cursorVisible;
        }

        private void ClearAll()
        {
            var bottom = _offscreenBuffer.CursorBottom;

            for (var i = 0; i < _offscreenBuffer.LineCount; i++)
            {
                _consoleDriver.ClearLine(bottom - i);
            }

            _offscreenBuffer.Clear();
        }
    }
}
