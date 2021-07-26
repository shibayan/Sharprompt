using System;

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

        public string ErrorMessage { get; set; }

        public void Dispose()
        {
            _consoleDriver.Dispose();
        }

        public void Render(Action<OffscreenBuffer> template)
        {
            _consoleDriver.CursorVisible = false;

            _offscreenBuffer.ClearConsole();

            template(_offscreenBuffer);

            if (ErrorMessage != null)
            {
                _offscreenBuffer.WriteErrorMessage(ErrorMessage);

                ErrorMessage = null;
            }

            _offscreenBuffer.RenderToConsole();

            _consoleDriver.CursorVisible = _cursorVisible;
        }

        public void Render<TModel>(Action<OffscreenBuffer, TModel> template, TModel result)
        {
            _consoleDriver.CursorVisible = false;

            _offscreenBuffer.ClearConsole();

            template(_offscreenBuffer, result);

            _offscreenBuffer.RenderToConsole();

            _consoleDriver.WriteLine();

            _consoleDriver.CursorVisible = _cursorVisible;
        }
    }
}
