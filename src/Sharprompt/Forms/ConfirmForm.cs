using System;

using Sharprompt.Internal;
using Sharprompt.Strings;

namespace Sharprompt.Forms;

/// <summary>
/// Reads a localized yes-or-no answer, with an optional default accepted by Enter.
/// </summary>
internal class ConfirmForm : TextFormBase<bool>
{
    /// <summary>
    /// Validates confirmation settings and creates the text-input prompt.
    /// </summary>
    /// <param name="options">The settings to validate and use for this prompt.</param>
    /// <param name="configuration">The symbols, colors, console factory, and cancellation settings to use.</param>
    public ConfirmForm(ConfirmOptions options, PromptConfiguration configuration) : base(configuration)
    {
        options.EnsureOptions();

        _options = options;
    }

    private readonly ConfirmOptions _options;

    /// <summary>
    /// Writes the prompt, localized yes/no hints, and current answer text.
    /// </summary>
    /// <param name="offscreenBuffer">The pending output buffer to write or render.</param>
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

    /// <summary>
    /// Writes the completed prompt with the accepted localized yes-or-no answer.
    /// </summary>
    /// <param name="offscreenBuffer">The pending output buffer to write or render.</param>
    /// <param name="result">The accepted result used to render the completed prompt.</param>
    protected override void FinishTemplate(OffscreenBuffer offscreenBuffer, bool result)
    {
        offscreenBuffer.WriteDone(_options.Message);
        offscreenBuffer.WriteAnswer(result ? Resource.ConfirmForm_Answer_Yes : Resource.ConfirmForm_Answer_No);
    }

    /// <summary>
    /// Accepts a localized answer or the configured default, or reports missing or invalid input.
    /// </summary>
    /// <param name="result">Receives the accepted result when the method returns true; otherwise it must not be consumed.</param>
    /// <returns>True when the prompt can finish with the result; otherwise false to continue prompting.</returns>
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
