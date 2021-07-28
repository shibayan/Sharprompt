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
                    case ConsoleKey.Enter when keyInfo.Modifiers == 0:
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

                                SetValidationResult(new ValidationResult(Prompt.Messages.Required));
                            }
                            else
                            {
                                var lowerInput = input.ToLower();

                                if (lowerInput == Prompt.Messages.YesKey.ToString().ToLower()
                                    || lowerInput == Prompt.Messages.LongYesKey.ToLower())
                                {
                                    result = true;

                                    return true;
                                }

                                if (lowerInput == Prompt.Messages.NoKey.ToString().ToLower()
                                    || lowerInput == Prompt.Messages.LongNoKey.ToLower())
                                {
                                    result = false;

                                    return true;
                                }

                                SetValidationResult(new ValidationResult(Prompt.Messages.Invalid));
                            }

                            break;
                        }
                    }
                    case ConsoleKey.LeftArrow when keyInfo.Modifiers == 0 && _startIndex > 0:
                        _startIndex -= 1;
                        break;
                    case ConsoleKey.RightArrow when keyInfo.Modifiers == 0 && _startIndex < _inputBuffer.Length:
                        _startIndex += 1;
                        break;
                    case ConsoleKey.Backspace when keyInfo.Modifiers == 0 && _startIndex > 0:
                        _startIndex -= 1;
                        _inputBuffer.Remove(_startIndex, 1);
                        break;
                    case ConsoleKey.Delete when !char.IsControl(keyInfo.KeyChar) && _startIndex < _inputBuffer.Length:
                        _inputBuffer.Remove(_startIndex, 1);
                        break;
                    default:
                    {
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            if (!char.IsControl(keyInfo.KeyChar) && keyInfo.Modifiers == 0)
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
                screenBuffer.Write($"({char.ToLower(Prompt.Messages.YesKey)}/{ char.ToLower(Prompt.Messages.NoKey)}) ");
            }
            else if (_options.DefaultValue.Value)
            {
                screenBuffer.Write($"({char.ToUpper(Prompt.Messages.YesKey)}/{char.ToLower(Prompt.Messages.NoKey)}) ");
            }
            else
            {
                screenBuffer.Write($"({char.ToLower(Prompt.Messages.YesKey)}/{char.ToUpper(Prompt.Messages.NoKey)}) ");
            }

            if (_options.DefaultValue.HasValue)
            {
                if (_initform && _options.StartWithDefaultValue)
                {
                    if (_options.DefaultValue.Value)
                    {
                        _inputBuffer.Append(Prompt.Messages.YesKey.ToString());
                    }
                    else
                    {
                        _inputBuffer.Append(Prompt.Messages.NoKey.ToString());
                    }
                    _startIndex = 1;
                }
            }

            var (left, top) = screenBuffer.GetCursorPosition();

            var input = _inputBuffer.ToString();

            screenBuffer.Write(input);

            var width = left + input.Take(_startIndex).GetWidth();

            screenBuffer.SetCursorPosition(width % screenBuffer.BufferWidth, top + (width / screenBuffer.BufferWidth));

            if (_options.ShowKeyNavigation)
            {
                screenBuffer.WriteLine();
                screenBuffer.Write(Prompt.Messages.ConfirmKeyNavigation, Prompt.ColorSchema.KeyNavigation);
            }

            _initform = false;
        }

        protected override void FinishTemplate(OffscreenBuffer screenBuffer, bool result)
        {
            screenBuffer.WriteFinish(_options.Message);
            screenBuffer.Write(result ? Prompt.Messages.YesKey.ToString() : Prompt.Messages.NoKey.ToString(), Prompt.ColorSchema.Answer);
        }
    }
}
