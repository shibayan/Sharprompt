using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using Sharprompt.Internal;

namespace Sharprompt.Forms
{
    internal class AnykeyForm : FormBase<bool>
    {
        private readonly ManualResetEvent _mreKeyPress;
        public AnykeyForm()
        {
            _mreKeyPress = new ManualResetEvent(false);
        }

        protected override bool TryGetResult(CancellationToken cancellationToken, out bool result)
        {
            while (!ConsoleDriver.KeyAvailable && !cancellationToken.IsCancellationRequested)
            {
                Thread.Sleep(Prompt.DefaultMessageValues.IdleReadKey);
            }
            if (ConsoleDriver.KeyAvailable && !cancellationToken.IsCancellationRequested)
            {
                _ = ConsoleDriver.ReadKey();
            }
            else
            {
                result = false;
                return false;
            }
            result = true;
            return true;
        }

        protected override void InputTemplate(OffscreenBuffer screenBuffer)
        {
            screenBuffer.Write(Prompt.DefaultMessageValues.DefaultAnyKeyMessage);
            screenBuffer.SetCursorPosition();
        }

        protected override void FinishTemplate(OffscreenBuffer screenBuffer, bool result)
        {
        }
    }
}
