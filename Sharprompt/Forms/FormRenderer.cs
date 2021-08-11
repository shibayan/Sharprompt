using System;

using Sharprompt.Drivers;
using Sharprompt.Internal;

namespace Sharprompt.Forms
{
    internal class FormRenderer : IDisposable
    {
        public FormRenderer(IConsoleDriver consoleDriver)
        {
            consoleDriver.RequestCancellation = RequestCancellation;

            _offscreenBuffer = new OffscreenBuffer(consoleDriver);
        }

        private readonly OffscreenBuffer _offscreenBuffer;

        public string ErrorMessage { get; set; }

        public void Dispose() => _offscreenBuffer.Dispose();

        public void Render(Action<OffscreenBuffer> template)
        {
            using (_offscreenBuffer.BeginRender())
            {
                template(_offscreenBuffer);

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

        private void RequestCancellation()
        {
            _offscreenBuffer.Cancel();

            if (Prompt.ThrowExceptionOnCancel)
            {
                throw new PromptCanceledException();
            }

            Environment.Exit(1);
        }
    }
}
