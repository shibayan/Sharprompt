using System;
using System.Collections.Generic;
using System.Linq;

using Sharprompt.Internal;
using Sharprompt.Strings;

namespace Sharprompt.Forms;

internal class ListForm<T> : FormBase<IEnumerable<T>>
{
    public ListForm(ListOptions<T> options)
    {
        options.EnsureOptions();

        _options = options;

        _inputItems.AddRange(options.DefaultValues ?? Enumerable.Empty<T>());
    }

    private readonly ListOptions<T> _options;

    private readonly List<T> _inputItems = new();
    private readonly TextInputBuffer _textInputBuffer = new();

    protected override bool TryGetResult(out IEnumerable<T> result)
    {
        do
        {
            var keyInfo = ConsoleDriver.ReadKey();

            switch (keyInfo.Key)
            {
                case ConsoleKey.Enter:
                    return HandleEnter(out result);
                case ConsoleKey.LeftArrow when !_textInputBuffer.IsStart:
                    _textInputBuffer.MoveBackward();
                    break;
                case ConsoleKey.RightArrow when !_textInputBuffer.IsEnd:
                    _textInputBuffer.MoveForward();
                    break;
                case ConsoleKey.Backspace when !_textInputBuffer.IsStart:
                    _textInputBuffer.Backspace();
                    break;
                case ConsoleKey.Delete when !_textInputBuffer.IsEnd:
                    _textInputBuffer.Delete();
                    break;
                case ConsoleKey.Delete when keyInfo.Modifiers == ConsoleModifiers.Control:
                    if (_inputItems.Any())
                    {
                        _inputItems.RemoveAt(_inputItems.Count - 1);
                    }
                    break;
                case ConsoleKey.LeftArrow:
                case ConsoleKey.RightArrow:
                case ConsoleKey.Backspace:
                case ConsoleKey.Delete:
                    ConsoleDriver.Beep();
                    break;
                default:
                    if (!char.IsControl(keyInfo.KeyChar))
                    {
                        _textInputBuffer.Insert(keyInfo.KeyChar);
                    }
                    break;
            }

        } while (ConsoleDriver.KeyAvailable);

        result = default;

        return false;
    }

    protected override void InputTemplate(OffscreenBuffer offscreenBuffer)
    {
        offscreenBuffer.WritePrompt(_options.Message);

        offscreenBuffer.WriteInput(_textInputBuffer);

        foreach (var inputItem in _inputItems)
        {
            offscreenBuffer.WriteLine();
            offscreenBuffer.Write($"  {inputItem}");
        }
    }

    protected override void FinishTemplate(OffscreenBuffer offscreenBuffer, IEnumerable<T> result)
    {
        offscreenBuffer.WriteDone(_options.Message);
        offscreenBuffer.WriteAnswer(string.Join(", ", result));
    }

    private bool HandleEnter(out IEnumerable<T> result)
    {
        var input = _textInputBuffer.ToString();

        try
        {
            result = _inputItems;

            if (string.IsNullOrEmpty(input))
            {
                if (_inputItems.Count >= _options.Minimum)
                {
                    return true;
                }

                SetError(string.Format(Resource.Validation_Minimum_InputRequired, _options.Minimum));

                return false;
            }

            if (_inputItems.Count >= _options.Maximum)
            {
                SetError(string.Format(Resource.Validation_Maximum_InputRequired, _options.Maximum));

                return false;
            }

            var inputValue = TypeHelper<T>.ConvertTo(input);

            if (!TryValidate(inputValue, _options.Validators))
            {
                return false;
            }

            _textInputBuffer.Clear();

            _inputItems.Add(inputValue);
        }
        catch (Exception ex)
        {
            SetError(ex);
        }

        result = default;

        return false;
    }
}
