using System;
using System.Diagnostics.CodeAnalysis;

using Sharprompt.Internal;
using Sharprompt.Strings;

namespace Sharprompt.Forms;

internal class InputForm<T> : TextFormBase<T>
{
    public InputForm(InputOptions<T> options)
    {
        options.EnsureOptions();

        _options = options;

        _defaultValue = Optional<T>.Create(options.DefaultValue);
    }

    private readonly InputOptions<T> _options;
    private readonly Optional<T> _defaultValue;

    protected override void InputTemplate(OffscreenBuffer offscreenBuffer)
    {
        offscreenBuffer.WritePrompt(_options.Message);

        if (_defaultValue.HasValue)
        {
            offscreenBuffer.WriteHint($"({_defaultValue.Value}) ");
        }

        if (InputBuffer.Length == 0 && !string.IsNullOrEmpty(_options.Placeholder))
        {
            offscreenBuffer.PushCursor();
            offscreenBuffer.WriteHint(_options.Placeholder);
        }

        offscreenBuffer.WriteInput(InputBuffer);
    }

    protected override void FinishTemplate(OffscreenBuffer offscreenBuffer, T result)
    {
        offscreenBuffer.WriteDone(_options.Message);

        if (result is not null)
        {
            offscreenBuffer.WriteAnswer(result.ToString()!);
        }
    }

    protected override bool HandleEnter([NotNullWhen(true)] out T? result)
    {
        var input = InputBuffer.ToString();

        try
        {
            if (string.IsNullOrEmpty(input))
            {
                if (!TypeHelper<T>.IsNullable && !_defaultValue.HasValue)
                {
                    SetError(Resource.Validation_Required);

                    result = default;

                    return false;
                }

                result = _defaultValue;
            }
            else
            {
                result = TypeHelper<T>.ConvertTo(input);
            }

            return TryValidate(result, _options.Validators);
        }
        catch (Exception ex)
        {
            SetError(ex);
        }

        result = default;

        return false;
    }
}
