using System;
using System.Collections.Generic;

using Sharprompt.Internal;

namespace Sharprompt.Forms;

internal abstract class SelectFormBase<TItem, TResult> : FormBase<TResult> where TItem : notnull
{
    protected SelectFormBase(PromptConfiguration configuration) : base(configuration)
    {
    }

    protected Paginator<TItem> Paginator { get; private set; } = null!;

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

    protected override bool HandleTextInput(ConsoleKeyInfo keyInfo)
    {
        InputBuffer.Insert(keyInfo.KeyChar);

        Paginator.UpdateFilter(InputBuffer.ToString());

        return true;
    }

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
