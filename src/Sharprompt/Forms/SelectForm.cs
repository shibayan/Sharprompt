using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Sharprompt.Internal;
using Sharprompt.Strings;

namespace Sharprompt.Forms;

internal class SelectForm<T> : SelectFormBase<T, T> where T : notnull
{
    public SelectForm(SelectOptions<T> options, PromptConfiguration configuration) : this(options, Prepare(options), configuration)
    {
    }

    private SelectForm(SelectOptions<T> options, (T[] Items, Optional<T> DefaultValue) prepared, PromptConfiguration configuration) : base(configuration)
    {
        _options = options;

        InitializePaginator(prepared.Items, options.PageSize, prepared.DefaultValue, options.TextSelector, options.LoopingSelection);
    }

    private readonly SelectOptions<T> _options;

    // Rendering and Enter must use the same policy, otherwise the highlighted row can contradict the result.
    // With a creation callback, a lone substring match must not be treated as an explicit choice.
    private bool AutoSelectSingleItem => _options.CreateCustomItem is null || string.IsNullOrEmpty(Paginator.FilterKeyword);

    // Runs before the base constructor creates the console driver, so invalid options throw without leaving it undisposed.
    private static (T[] Items, Optional<T> DefaultValue) Prepare(SelectOptions<T> options)
    {
        options.EnsureOptions();

        // Items are enumerated once; equal items are listed once, at the position of the first.
        var items = options.Items.Distinct().ToArray();

        // Nothing could be selected, so the prompt could only be canceled. A creation callback
        // makes an empty list meaningful: every answer comes from the typed text.
        if (items.Length == 0 && options.CreateCustomItem is null)
        {
            throw new ArgumentException(Resource.Validation_Items_Empty, nameof(options.Items));
        }

        return (items, Optional<T>.Create(options.DefaultValue, nameof(options.DefaultValue)));
    }

    protected override void InputTemplate(OffscreenBuffer offscreenBuffer)
    {
        Paginator.UpdatePageSize(Math.Min(_options.PageSize, Height - 2));

        offscreenBuffer.WritePrompt(_options.Message);
        offscreenBuffer.Write(Paginator.FilterKeyword);

        offscreenBuffer.PushCursor();

        var hasSelected = Paginator.TryGetSelectedItem(AutoSelectSingleItem, out var selectedItem);
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
        if (Paginator.TryGetSelectedItem(AutoSelectSingleItem, out result))
        {
            return true;
        }

        var filter = Paginator.FilterKeyword;

        if (_options.CreateCustomItem is { } createCustomItem && !string.IsNullOrEmpty(filter))
        {
            try
            {
                var createdItem = createCustomItem(filter);

                if (createdItem is null)
                {
                    SetError(Resource.Validation_Invalid);
                    result = default;

                    return false;
                }

                result = createdItem;

                return true;
            }
            catch (Exception exception)
            {
                SetError(exception);
                result = default;

                return false;
            }
        }

        SetError(Resource.Validation_Required);

        return false;
    }
}
