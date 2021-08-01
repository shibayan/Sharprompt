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
            using (_offscreenBuffer.BeginRender())
            {
                template(_offscreenBuffer);

                _offscreenBuffer.PushCursor();

                if (ErrorMessage != null)
                {
                    _offscreenBuffer.WriteErrorMessage(ErrorMessage);

                    ErrorMessage = null;
                }
            }
        }

        public void Render<TModel>(Action<OffscreenBuffer, TModel> template, TModel result)
        {
            using (_offscreenBuffer.BeginRender())
            {
                template(_offscreenBuffer, result);

                _offscreenBuffer.WriteLine();
            }
        }
    }
}
