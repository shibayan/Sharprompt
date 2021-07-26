using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using Sharprompt.Internal;

namespace Sharprompt.Forms
{
    internal class AnykeyForm : FormBase<bool>
    {
        public AnykeyForm()
        {
        }

        protected override bool TryGetResult(CancellationToken cancellationToken, out bool result)
        {
            _ = ConsoleDriver.WaitKeypress(cancellationToken);
            if (cancellationToken.IsCancellationRequested)
            {
                result = false;
                return false;
            }
            result = true;
            return true;
        }

        protected override void InputTemplate(OffscreenBuffer screenBuffer)
        {
            screenBuffer.Write(Prompt.Messages.AnyKey);
            screenBuffer.SetCursorPosition();
        }

        protected override void FinishTemplate(OffscreenBuffer screenBuffer, bool result)
        {
        }
    }
}
