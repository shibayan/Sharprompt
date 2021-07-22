using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading;

using Sharprompt.Internal;

namespace Sharprompt.Forms
{
    internal class ConfirmForm : FormBase<bool>
    {
        private bool _initform;
        public ConfirmForm(ConfirmOptions options)
        {
            _options = options;
            _initform = true;
        }

        private readonly ConfirmOptions _options;

        protected override bool TryGetResult(CancellationToken cancellationToken, out bool result)
        {
            ConsoleKeyInfo keyInfo;
            while (!ConsoleDriver.KeyAvailable && !cancellationToken.IsCancellationRequested)
            {
                Thread.Sleep(Prompt.DefaultMessageValues.IdleReadKey);
            }
            if (ConsoleDriver.KeyAvailable && !cancellationToken.IsCancellationRequested)
            {
                keyInfo = ConsoleDriver.ReadKey();
            }
            else
            {
                if (_options.DefaultValue != null)
                {
                    result = _options.DefaultValue.Value;
                    return false;
                }
                result = false;
                return false;
            }

            if (keyInfo.Key == ConsoleKey.Enter)
            {
                if (_options.DefaultValue != null)
                {
                    result = _options.DefaultValue.Value;

                    return true;
                }
            }
            else
            {
                var lowerInput = char.ToLower(keyInfo.KeyChar);

                if (lowerInput == char.ToLower(Prompt.DefaultMessageValues.DefaultYesKey))
                {
                    result = true;

                    return true;
                }

                if (lowerInput == char.ToLower(Prompt.DefaultMessageValues.DefaultNoKey))
                {
                    result = false;

                    return true;
                }
            }

            if (!char.IsControl(keyInfo.KeyChar))
            {
                Renderer.SetValidationResult(new ValidationResult(Prompt.DefaultMessageValues.DefaultInvalidValueMessage));
            }

            result = default;

            return false;
        }

        protected override void InputTemplate(OffscreenBuffer screenBuffer)
        {

            var input = string.Empty;

            screenBuffer.WritePrompt(_options.Message);

            if (_options.DefaultValue.HasValue)
            {
                if (_initform)
                {
                    if (_options.DefaultValue.Value)
                    {
                        input = Prompt.DefaultMessageValues.DefaultYesKey.ToString();
                    }
                    else
                    {
                        input = Prompt.DefaultMessageValues.DefaultNoKey.ToString();
                    }
                }
            }

            if (_options.DefaultValue == null)
            {
                screenBuffer.Write($"({char.ToLower(Prompt.DefaultMessageValues.DefaultYesKey)}/{ char.ToLower(Prompt.DefaultMessageValues.DefaultNoKey)}) ");
            }
            else if (_options.DefaultValue.Value)
            {
                screenBuffer.Write($"({char.ToUpper(Prompt.DefaultMessageValues.DefaultYesKey)}/{char.ToLower(Prompt.DefaultMessageValues.DefaultNoKey)}) ");
            }
            else
            {
                screenBuffer.Write($"({char.ToLower(Prompt.DefaultMessageValues.DefaultYesKey)}/{char.ToUpper(Prompt.DefaultMessageValues.DefaultNoKey)}) ");
            }

            var (left, top) = screenBuffer.GetCursorPosition();

            screenBuffer.Write(input);

            var startIndex = 0;
            if (_initform)
            {
                startIndex = input.Length;
            }

            var width = EastAsianWidth.GetWidth(input.Take(startIndex)) + left;

            screenBuffer.SetCursorPosition(width % screenBuffer.BufferWidth, top + (width / screenBuffer.BufferWidth));

            _initform = false;
        }

        protected override void FinishTemplate(OffscreenBuffer screenBuffer, bool result)
        {
            screenBuffer.WriteFinish(_options.Message);
            screenBuffer.Write(result ? Prompt.DefaultMessageValues.DefaultYesKey.ToString() : Prompt.DefaultMessageValues.DefaultNoKey.ToString(), Prompt.ColorSchema.Answer);
        }
    }
}
