using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Sharprompt.Internal;
using Sharprompt.Strings;

namespace Sharprompt.Forms;

internal class ListForm<T> : TextFormBase<IEnumerable<T>> where T : notnull
{
    public ListForm(ListOptions<T> options)
    {
        options.EnsureOptions();

        _options = options;

        _inputItems.AddRange(options.DefaultValues);
    }

    private readonly ListOptions<T> _options;

    private readonly List<T> _inputItems = [];

    protected override void InputTemplate(OffscreenBuffer offscreenBuffer)
    {
        offscreenBuffer.WritePrompt(_options.Message);

        offscreenBuffer.WriteInput(InputBuffer);

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

    protected override bool HandleEnter([NotNullWhen(true)] out IEnumerable<T>? result)
    {
        var input = InputBuffer.ToString();

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

            InputBuffer.Clear();

            _inputItems.Add(inputValue);
        }
        catch (Exception ex)
        {
            SetError(ex);
        }

        result = default;

        return false;
    }

    protected override bool HandleDelete(ConsoleKeyInfo keyInfo)
    {
        if (keyInfo.Modifiers == ConsoleModifiers.Control && _inputItems.Any())
        {
            _inputItems.RemoveAt(_inputItems.Count - 1);

            return true;
        }

        return base.HandleDelete(keyInfo);
    }
}
