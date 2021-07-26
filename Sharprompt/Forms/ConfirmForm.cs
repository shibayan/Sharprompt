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
        private int _startIndex;
        private readonly StringBuilder _inputBuffer = new StringBuilder();

        protected override bool TryGetResult(CancellationToken cancellationToken, out bool result)
        {
            do
            {
                var keyInfo = ConsoleDriver.WaitKeypress(cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                {
                    if (_options.DefaultValue != null)
                    {
                        result = _options.DefaultValue.Value;
                        return false;
                    }
                    result = false;
                    return false;
                }

                switch (keyInfo.Key)
                {
                    case ConsoleKey.Enter:
                    {
                        {
                            var input = _inputBuffer.ToString();

                            if (string.IsNullOrEmpty(input))
                            {
                                if (_options.DefaultValue != null)
                                {
                                    result = _options.DefaultValue.Value;

                                    return true;
                                }

                                SetValidationResult(new ValidationResult(Prompt.DefaultMessageValues.DefaultRequiredMessage));
                            }
                            else
                            {
                                var lowerInput = input.ToLower();

                                if (lowerInput == Prompt.DefaultMessageValues.DefaultYesKey.ToString().ToLower()
                                    || lowerInput == Prompt.DefaultMessageValues.DefaultLongYesKey.ToLower())
                                {
                                    result = true;

                                    return true;
                                }

                                if (lowerInput == Prompt.DefaultMessageValues.DefaultNoKey.ToString().ToLower()
                                    || lowerInput == Prompt.DefaultMessageValues.DefaultLongNoKey.ToLower())
                                {
                                    result = false;

                                    return true;
                                }

                                SetValidationResult(new ValidationResult(Prompt.DefaultMessageValues.DefaultInvalidValueMessage));
                            }

                            break;
                        }
                    }
                    case ConsoleKey.LeftArrow when _startIndex > 0:
                        _startIndex -= 1;
                        break;
                    case ConsoleKey.RightArrow when _startIndex < _inputBuffer.Length:
                        _startIndex += 1;
                        break;
                    case ConsoleKey.Backspace when _startIndex > 0:
                        _startIndex -= 1;

                        _inputBuffer.Remove(_startIndex, 1);
                        break;
                    case ConsoleKey.Delete when _startIndex < _inputBuffer.Length:
                        _inputBuffer.Remove(_startIndex, 1);
                        break;
                    case ConsoleKey.LeftArrow:
                    case ConsoleKey.RightArrow:
                    case ConsoleKey.Backspace:
                    case ConsoleKey.Delete:
                        ConsoleDriver.Beep();
                        break;
                    default:
                    {
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            if (!char.IsControl(keyInfo.KeyChar))
                            {
                                _inputBuffer.Insert(_startIndex, keyInfo.KeyChar);

                                _startIndex += 1;
                            }
                        }
                        break;
                    }
                }

            } while (ConsoleDriver.KeyAvailable && !cancellationToken.IsCancellationRequested);

            result = default;

            return false;
        }

        protected override void InputTemplate(OffscreenBuffer screenBuffer)
        {

            screenBuffer.WritePrompt(_options.Message);

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

            if (_options.DefaultValue.HasValue)
            {
                if (_initform && _options.StartWithDefaultValue)
                {
                    if (_options.DefaultValue.Value)
                    {
                        _inputBuffer.Append(Prompt.DefaultMessageValues.DefaultYesKey.ToString());
                    }
                    else
                    {
                        _inputBuffer.Append(Prompt.DefaultMessageValues.DefaultNoKey.ToString());
                    }
                    _startIndex = 1;
                }
            }

            var (left, top) = screenBuffer.GetCursorPosition();

            var input = _inputBuffer.ToString();

            screenBuffer.Write(input);

            var width = left + input.Take(_startIndex).GetWidth();

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
