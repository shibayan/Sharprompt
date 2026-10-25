using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Sharprompt.Internal;
using Sharprompt.Strings;

namespace Sharprompt.Forms;

/// <summary>
/// Selects one original item from a filtered, paginated snapshot of distinct items.
/// </summary>
/// <typeparam name="T">The result type returned by the prompt or template.</typeparam>
internal class SelectForm<T> : SelectFormBase<T, T> where T : notnull
{
    /// <summary>
    /// Validates and snapshots distinct items before creating the driver and initializing selection.
    /// </summary>
    /// <param name="options">The settings to validate and use for this prompt.</param>
    /// <param name="configuration">The symbols, colors, console factory, and cancellation settings to use.</param>
    public SelectForm(SelectOptions<T> options, PromptConfiguration configuration) : this(options, Prepare(options), configuration)
    {
    }

    private SelectForm(SelectOptions<T> options, (T[] Items, Optional<T> DefaultValue) prepared, PromptConfiguration configuration) : base(configuration)
    {
        _options = options;

        InitializePaginator(prepared.Items, options.PageSize, prepared.DefaultValue, options.TextSelector, options.LoopingSelection);
    }

    private readonly SelectOptions<T> _options;

    // Runs before the base constructor creates the console driver, so invalid options throw without leaving it undisposed.
    private static (T[] Items, Optional<T> DefaultValue) Prepare(SelectOptions<T> options)
    {
        options.EnsureOptions();

        // Items are enumerated once; equal items are listed once, at the position of the first.
        var items = options.Items.Distinct().ToArray();

        // Nothing could be selected, so the prompt could only be canceled.
        if (items.Length == 0)
        {
            throw new ArgumentException(Resource.Validation_Items_Empty, nameof(options.Items));
        }

        return (items, Optional<T>.Create(options.DefaultValue, nameof(options.DefaultValue)));
    }

    /// <summary>
    /// Writes the filter, visible page, highlighted item, and pagination hint.
    /// </summary>
    /// <param name="offscreenBuffer">The pending output buffer to write or render.</param>
    protected override void InputTemplate(OffscreenBuffer offscreenBuffer)
    {
        Paginator.UpdatePageSize(Math.Min(_options.PageSize, Height - 2));

        offscreenBuffer.WritePrompt(_options.Message);
        offscreenBuffer.Write(Paginator.FilterKeyword);

        offscreenBuffer.PushCursor();

        var hasSelected = Paginator.TryGetSelectedItem(out var selectedItem);
        var comparer = EqualityComparer<T>.Default;

        foreach (var item in Paginator.CurrentItems)
        {
            var value = _options.TextSelector(item);

            offscreenBuffer.WriteLine();

            if (hasSelected && comparer.Equals(item, selectedItem))
            {
                offscreenBuffer.WriteSelect($"{Configuration.Symbols.Selector} {value}");
            }
            else
            {
                offscreenBuffer.Write($"  {value}");
            }
        }

        RenderPagination(offscreenBuffer, _options.Pagination);
    }

    /// <summary>
    /// Writes the completed prompt with the selected item display text.
    /// </summary>
    /// <param name="offscreenBuffer">The pending output buffer to write or render.</param>
    /// <param name="result">The accepted result used to render the completed prompt.</param>
    protected override void FinishTemplate(OffscreenBuffer offscreenBuffer, T result)
    {
        offscreenBuffer.WriteDone(_options.Message);
        offscreenBuffer.WriteAnswer(_options.TextSelector(result));
    }

    /// <summary>
    /// Accepts the highlighted or uniquely matched item, otherwise reports that a selection is required.
    /// </summary>
    /// <param name="result">Receives the accepted result when the method returns true; otherwise it must not be consumed.</param>
    /// <returns>True when the prompt can finish with the result; otherwise false to continue prompting.</returns>
    protected override bool HandleEnter([NotNullWhen(true)] out T? result)
    {
        if (Paginator.TryGetSelectedItem(out result))
        {
            return true;
        }

        SetError(Resource.Validation_Required);

        return false;
    }
}
