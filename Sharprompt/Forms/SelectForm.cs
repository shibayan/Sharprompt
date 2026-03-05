using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using Sharprompt.Internal;
using Sharprompt.Strings;

namespace Sharprompt.Forms;

internal class SelectForm<T> : SelectFormBase<T, T> where T : notnull
{
    public SelectForm(SelectOptions<T> options, PromptConfiguration configuration) : base(configuration)
    {
        options.EnsureOptions();

        _options = options;

        InitializePaginator(options.Items, options.PageSize, Optional<T>.Create(options.DefaultValue), options.TextSelector, options.LoopingSelection);
    }

    private readonly SelectOptions<T> _options;

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

    protected override void FinishTemplate(OffscreenBuffer offscreenBuffer, T result)
    {
        offscreenBuffer.WriteDone(_options.Message);
        offscreenBuffer.WriteAnswer(_options.TextSelector(result));
    }

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
