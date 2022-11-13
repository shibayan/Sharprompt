using System;

using Sharprompt.Internal;
using Sharprompt.Strings;

namespace Sharprompt.Forms;

internal class ConfirmForm : TextFormBase<bool>
{
    public ConfirmForm(ConfirmOptions options)
    {
        options.EnsureOptions();

        _options = options;
    }

    private readonly ConfirmOptions _options;

    protected override void InputTemplate(OffscreenBuffer offscreenBuffer)
    {
        offscreenBuffer.WritePrompt(_options.Message);

        var answerYes = char.ToLowerInvariant(Resource.ConfirmForm_Answer_Yes[0]);
        var answerNo = char.ToLowerInvariant(Resource.ConfirmForm_Answer_No[0]);

        if (_options.DefaultValue.HasValue)
        {
            if (_options.DefaultValue.Value)
            {
                answerYes = char.ToUpperInvariant(answerYes);
            }
            else
            {
                answerNo = char.ToUpperInvariant(answerNo);
            }
        }

        offscreenBuffer.WriteHint($"({answerYes}/{answerNo}) ");
        offscreenBuffer.WriteInput(InputBuffer);
    }

    protected override void FinishTemplate(OffscreenBuffer offscreenBuffer, bool result)
    {
        offscreenBuffer.WriteDone(_options.Message);
        offscreenBuffer.WriteAnswer(result ? Resource.ConfirmForm_Answer_Yes : Resource.ConfirmForm_Answer_No);
    }

    protected override bool HandleEnter(out bool result)
    {
        var input = InputBuffer.ToString();

        if (string.IsNullOrEmpty(input))
        {
            if (_options.DefaultValue.HasValue)
            {
                result = _options.DefaultValue.Value;

                return true;
            }

            SetError(Resource.Validation_Required);
        }
        else
        {
            if (input.Equals(Resource.ConfirmForm_Answer_Yes, StringComparison.OrdinalIgnoreCase) ||
                input.Equals(Resource.ConfirmForm_Answer_Yes[..1], StringComparison.OrdinalIgnoreCase))
            {
                result = true;

                return true;
            }

            if (input.Equals(Resource.ConfirmForm_Answer_No, StringComparison.OrdinalIgnoreCase) ||
                input.Equals(Resource.ConfirmForm_Answer_No[..1], StringComparison.OrdinalIgnoreCase))
            {
                result = false;

                return true;
            }

            InputBuffer.Clear();

            SetError(Resource.Validation_Invalid);
        }

        result = default;

        return false;
    }
}
