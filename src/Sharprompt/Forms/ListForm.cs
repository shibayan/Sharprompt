using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Sharprompt.Internal;
using Sharprompt.Strings;

namespace Sharprompt.Forms;

/// <summary>
/// Collects converted and validated values until an empty line completes the list.
/// </summary>
/// <typeparam name="T">The result type returned by the prompt or template.</typeparam>
internal class ListForm<T> : TextFormBase<IEnumerable<T>> where T : notnull
{
    /// <summary>
    /// Validates list limits and copies the initial values into the list being edited.
    /// </summary>
    /// <param name="options">The settings to validate and use for this prompt.</param>
    /// <param name="configuration">The symbols, colors, console factory, and cancellation settings to use.</param>
    public ListForm(ListOptions<T> options, PromptConfiguration configuration) : base(configuration)
    {
        options.EnsureOptions();

        _options = options;

        _inputItems.AddRange(options.DefaultValues);
    }

    private readonly ListOptions<T> _options;

    private readonly List<T> _inputItems = [];

    /// <summary>
    /// Writes the editable input and the invariant-culture text of items collected so far.
    /// </summary>
    /// <param name="offscreenBuffer">The pending output buffer to write or render.</param>
    protected override void InputTemplate(OffscreenBuffer offscreenBuffer)
    {
        offscreenBuffer.WritePrompt(_options.Message);

        offscreenBuffer.WriteInput(InputBuffer);

        foreach (var inputItem in _inputItems)
        {
            offscreenBuffer.WriteLine();
            offscreenBuffer.Write($"  {TypeHelper<T>.ConvertToString(inputItem)}");
        }
    }

    /// <summary>
    /// Writes the completed prompt with the invariant-culture text of the collected items.
    /// </summary>
    /// <param name="offscreenBuffer">The pending output buffer to write or render.</param>
    /// <param name="result">The accepted result used to render the completed prompt.</param>
    protected override void FinishTemplate(OffscreenBuffer offscreenBuffer, IEnumerable<T> result)
    {
        offscreenBuffer.WriteDone(_options.Message);
        offscreenBuffer.WriteAnswer(string.Join(", ", result.Select(TypeHelper<T>.ConvertToString)));
    }

    /// <summary>
    /// Adds a converted, validated item, or finishes on an empty line when the minimum count is satisfied.
    /// </summary>
    /// <param name="result">Receives the accepted result when the method returns true; otherwise it must not be consumed.</param>
    /// <returns>True when the prompt can finish with the result; otherwise false to continue prompting.</returns>
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

    /// <summary>
    /// Removes the last collected item, falling back to forward word deletion when the list is empty.
    /// </summary>
    /// <returns>True when the key changed or handled the input; otherwise false.</returns>
    protected override bool HandleCtrlDelete()
    {
        if (_inputItems.Count > 0)
        {
            _inputItems.RemoveAt(_inputItems.Count - 1);

            return true;
        }

        return base.HandleCtrlDelete();
    }
}
