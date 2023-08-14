using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using Sharprompt.Internal;
using Sharprompt.Strings;

namespace Sharprompt.Forms;

internal class SelectForm<T> : FormBase<T> where T : notnull
{
    public SelectForm(SelectOptions<T> options)
    {
        options.EnsureOptions();

        _options = options;
        _paginator = new Paginator<T>(options.Items, Math.Min(options.PageSize, Height - 2), Optional<T>.Create(options.DefaultValue), options.TextSelector)
        {
            LoopingSelection = options.LoopingSelection
        };

        KeyHandlerMaps = new()
        {
            [ConsoleKey.UpArrow] = HandleUpArrow,
            [ConsoleKey.DownArrow] = HandleDownArrow,
            [ConsoleKey.LeftArrow] = HandleLeftArrow,
            [ConsoleKey.RightArrow] = HandleRightArrow,
            [ConsoleKey.Backspace] = HandleBackspace
        };
    }

    private readonly SelectOptions<T> _options;
    private readonly Paginator<T> _paginator;

    protected override void InputTemplate(OffscreenBuffer offscreenBuffer)
    {
        _paginator.UpdatePageSize(Math.Min(_options.PageSize, Height - 2));

        offscreenBuffer.WritePrompt(_options.Message);
        offscreenBuffer.Write(_paginator.FilterKeyword);

        offscreenBuffer.PushCursor();

        foreach (var item in _paginator.CurrentItems)
        {
            var value = _options.TextSelector(item);

            offscreenBuffer.WriteLine();

            if (_paginator.TryGetSelectedItem(out var selectedItem) && EqualityComparer<T>.Default.Equals(item, selectedItem))
            {
                offscreenBuffer.WriteSelect($"{Prompt.Symbols.Selector} {value}");
            }
            else
            {
                offscreenBuffer.Write($"  {value}");
            }
        }

        if (_paginator.PageCount > 1)
        {
            offscreenBuffer.WriteLine();
            offscreenBuffer.WriteHint(_options.Pagination(_paginator.TotalCount, _paginator.CurrentPage + 1, _paginator.PageCount));
        }
    }

    protected override void FinishTemplate(OffscreenBuffer offscreenBuffer, T result)
    {
        offscreenBuffer.WriteDone(_options.Message);
        offscreenBuffer.WriteAnswer(_options.TextSelector(result));
    }

    protected override bool HandleEnter([NotNullWhen(true)] out T? result)
    {
        if (_paginator.TryGetSelectedItem(out result))
        {
            return true;
        }

        SetError(Resource.Validation_Required);

        return false;
    }

    protected override bool HandleTextInput(ConsoleKeyInfo keyInfo)
    {
        base.HandleTextInput(keyInfo);

        _paginator.UpdateFilter(InputBuffer.ToString());

        return true;
    }

    private bool HandleUpArrow(ConsoleKeyInfo keyInfo)
    {
        _paginator.PreviousItem();

        return true;
    }

    private bool HandleDownArrow(ConsoleKeyInfo keyInfo)
    {
        _paginator.NextItem();

        return true;
    }

    private bool HandleLeftArrow(ConsoleKeyInfo keyInfo)
    {
        _paginator.PreviousPage();

        return true;
    }

    private bool HandleRightArrow(ConsoleKeyInfo keyInfo)
    {
        _paginator.NextPage();

        return true;
    }

    private bool HandleBackspace(ConsoleKeyInfo keyInfo)
    {
        if (InputBuffer.IsStart)
        {
            return false;
        }

        InputBuffer.Backspace();
        _paginator.UpdateFilter(InputBuffer.ToString());

        return true;
    }
}
