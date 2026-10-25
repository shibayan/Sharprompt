using System;
using System.Collections.Generic;

using Sharprompt.Internal;

namespace Sharprompt.Forms;

/// <summary>
/// Shares filtering, pagination, and navigation between single-selection and multiple-selection prompts.
/// </summary>
/// <typeparam name="TItem">The selection item type.</typeparam>
/// <typeparam name="TResult">The result type returned by the prompt or template.</typeparam>
internal abstract class SelectFormBase<TItem, TResult> : FormBase<TResult> where TItem : notnull
{
    /// <summary>
    /// Creates a selection prompt using the supplied rendering and console configuration.
    /// </summary>
    /// <param name="configuration">The symbols, colors, console factory, and cancellation settings to use.</param>
    protected SelectFormBase(PromptConfiguration configuration) : base(configuration)
    {
    }

    /// <summary>
    /// The filtered item snapshot, page state, and highlighted item used by selection handlers.
    /// </summary>
    protected Paginator<TItem> Paginator { get; private set; } = null!;

    /// <summary>
    /// Creates a paginator limited by terminal height and registers navigation and filter-editing keys.
    /// </summary>
    /// <param name="items">The items to snapshot and display in their supplied order.</param>
    /// <param name="pageSize">The requested items per page, normalized to at least one item.</param>
    /// <param name="defaultValue">The optional item to highlight initially.</param>
    /// <param name="textSelector">Formats items for display, filtering, and exact-match selection.</param>
    /// <param name="loopingSelection">Whether item navigation wraps within the current page.</param>
    protected void InitializePaginator(IEnumerable<TItem> items, int pageSize, Optional<TItem> defaultValue, Func<TItem, string> textSelector, bool loopingSelection)
    {
        Paginator = new Paginator<TItem>(items, Math.Min(pageSize, Height - 2), defaultValue, textSelector)
        {
            LoopingSelection = loopingSelection
        };

        KeyHandlerMaps[new ConsoleKeyBinding(ConsoleKey.UpArrow)] = HandleUpArrow;
        KeyHandlerMaps[new ConsoleKeyBinding(ConsoleKey.DownArrow)] = HandleDownArrow;
        KeyHandlerMaps[new ConsoleKeyBinding(ConsoleKey.LeftArrow)] = HandleLeftArrow;
        KeyHandlerMaps[new ConsoleKeyBinding(ConsoleKey.RightArrow)] = HandleRightArrow;
        KeyHandlerMaps[new ConsoleKeyBinding(ConsoleKey.Backspace)] = HandleBackspace;
    }

    /// <summary>
    /// Appends the typed character to the filter and recalculates matching items.
    /// </summary>
    /// <param name="keyInfo">The character and key information supplied by the console driver.</param>
    /// <returns>True when the key changed or handled the input; otherwise false.</returns>
    protected override bool HandleTextInput(ConsoleKeyInfo keyInfo)
    {
        InputBuffer.Insert(keyInfo.KeyChar);

        Paginator.UpdateFilter(InputBuffer.ToString());

        return true;
    }

    /// <summary>
    /// Writes the pagination hint only when the filtered items span multiple pages.
    /// </summary>
    /// <param name="offscreenBuffer">The pending output buffer to write or render.</param>
    /// <param name="pagination">Formats the total filtered item count, one-based page number, and page count.</param>
    protected void RenderPagination(OffscreenBuffer offscreenBuffer, Func<int, int, int, string> pagination)
    {
        if (Paginator.PageCount > 1)
        {
            offscreenBuffer.WriteLine();
            offscreenBuffer.WriteHint(pagination(Paginator.TotalCount, Paginator.CurrentPage + 1, Paginator.PageCount));
        }
    }

    private bool HandleUpArrow()
    {
        Paginator.PreviousItem();

        return true;
    }

    private bool HandleDownArrow()
    {
        Paginator.NextItem();

        return true;
    }

    private bool HandleLeftArrow()
    {
        Paginator.PreviousPage();

        return true;
    }

    private bool HandleRightArrow()
    {
        Paginator.NextPage();

        return true;
    }

    private bool HandleBackspace()
    {
        if (InputBuffer.IsStart)
        {
            return false;
        }

        InputBuffer.Backspace();

        Paginator.UpdateFilter(InputBuffer.ToString());

        return true;
    }
}
