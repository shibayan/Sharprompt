using System;

using Sharprompt.Drivers;
using Sharprompt.Internal;

namespace Sharprompt.Forms
{
    internal class FormRenderer : IDisposable
    {
        public FormRenderer(IConsoleDriver consoleDriver)
        {
            _consoleDriver = consoleDriver;

            _offscreenBuffer = new OffscreenBuffer(_consoleDriver);
        }

        private readonly IConsoleDriver _consoleDriver;
        private readonly OffscreenBuffer _offscreenBuffer;

        public string ErrorMessage { get; set; }

        public void Dispose() => _consoleDriver.Dispose();

        public void Render(Action<OffscreenBuffer> template)
        {
            _consoleDriver.CursorVisible = false;

            _offscreenBuffer.ClearConsole();

            template(_offscreenBuffer);

            _offscreenBuffer.PushCursor();

            if (ErrorMessage != null)
            {
                _offscreenBuffer.WriteErrorMessage(ErrorMessage);

                ErrorMessage = null;
            }

            _offscreenBuffer.RenderToConsole();

            _consoleDriver.CursorVisible = true;
        }

        public void Render<TModel>(Action<OffscreenBuffer, TModel> template, TModel result)
        {
            _consoleDriver.CursorVisible = false;

            _offscreenBuffer.ClearConsole();

            template(_offscreenBuffer, result);

            _offscreenBuffer.RenderToConsole();

            _consoleDriver.WriteLine();

            _consoleDriver.CursorVisible = true;
        }
    }
}
