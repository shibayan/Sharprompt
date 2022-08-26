using System;
using System.Collections.Generic;

using Sharprompt.Internal;
using Sharprompt.Strings;

namespace Sharprompt.Forms;

internal class SelectForm<T> : FormBase<T>
{
    public SelectForm(SelectOptions<T> options)
    {
        options.EnsureOptions();

        _options = options;

        var maxPageSize = ConsoleDriver.WindowHeight - 2;
        var pageSize = Math.Min(options.PageSize ?? maxPageSize, maxPageSize);

        _paginator = new Paginator<T>(options.Items, pageSize, Optional<T>.Create(options.DefaultValue), options.TextSelector);
    }

    private readonly SelectOptions<T> _options;
    private readonly Paginator<T> _paginator;

    private readonly TextInputBuffer _filterBuffer = new();

    protected override bool TryGetResult(out T result)
    {
        do
        {
            var keyInfo = ConsoleDriver.ReadKey();

            switch (keyInfo.Key)
            {
                case ConsoleKey.Enter when _paginator.TryGetSelectedItem(out result):
                    return true;
                case ConsoleKey.Enter:
                    SetError(Resource.Validation_Required);
                    break;
                case ConsoleKey.UpArrow:
                    _paginator.PreviousItem();
                    break;
                case ConsoleKey.DownArrow:
                    _paginator.NextItem();
                    break;
                case ConsoleKey.LeftArrow:
                    _paginator.PreviousPage();
                    break;
                case ConsoleKey.RightArrow:
                    _paginator.NextPage();
                    break;
                case ConsoleKey.Backspace when !_filterBuffer.IsStart:
                    _filterBuffer.Backspace();

                    _paginator.UpdateFilter(_filterBuffer.ToString());
                    break;
                case ConsoleKey.Backspace:
                    ConsoleDriver.Beep();
                    break;
                default:
                    if (!char.IsControl(keyInfo.KeyChar))
                    {
                        _filterBuffer.Insert(keyInfo.KeyChar);

                        _paginator.UpdateFilter(_filterBuffer.ToString());
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
        offscreenBuffer.Write(_paginator.FilterTerm);

        offscreenBuffer.PushCursor();

        var subset = _paginator.ToSubset();

        foreach (var item in subset)
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

        if (_paginator.PageCount > 1 && _options.Pagination is not null)
        {
            offscreenBuffer.WriteLine();
            offscreenBuffer.WriteHint(_options.Pagination(_paginator.TotalCount, _paginator.SelectedPage + 1, _paginator.PageCount));
        }
    }

    protected override void FinishTemplate(OffscreenBuffer offscreenBuffer, T result)
    {
        offscreenBuffer.WriteDone(_options.Message);
        offscreenBuffer.WriteAnswer(_options.TextSelector(result));
    }
}
