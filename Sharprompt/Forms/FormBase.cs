using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Sharprompt.Drivers;
using Sharprompt.Internal;
using Sharprompt.Strings;

namespace Sharprompt.Forms;

internal abstract class FormBase<T> : IDisposable
{
    protected FormBase()
    {
        ConsoleDriver = new DefaultConsoleDriver
        {
            CancellationCallback = CancellationHandler
        };

        _formRenderer = new FormRenderer(ConsoleDriver);
    }

    private readonly FormRenderer _formRenderer;

    protected IConsoleDriver ConsoleDriver { get; }

    protected TextInputBuffer InputBuffer { get; } = new();

    protected Dictionary<ConsoleKey, Func<ConsoleKeyInfo, bool>> KeyHandlerMaps { get; set; } = new();

    public void Dispose() => _formRenderer.Dispose();

    public T Start()
    {
        while (true)
        {
            _formRenderer.Render(InputTemplate);

            if (!TryGetResult(out var result))
            {
                continue;
            }

            _formRenderer.Render(FinishTemplate, result);

            return result;
        }
    }

    protected bool TryGetResult([NotNullWhen(true)] out T? result)
    {
        do
        {
            var keyInfo = ConsoleDriver.ReadKey();

            if (keyInfo.Key == ConsoleKey.Enter)
            {
                return HandleEnter(out result);
            }

            if (KeyHandlerMaps.TryGetValue(keyInfo.Key, out var keyHandler))
            {
                if (keyHandler(keyInfo))
                {
                    continue;
                }
            }

            if (!char.IsControl(keyInfo.KeyChar))
            {
                HandleTextInput(keyInfo);
            }
            else
            {
                ConsoleDriver.Beep();
            }

        } while (ConsoleDriver.KeyAvailable);

        result = default;

        return false;
    }

    protected abstract void InputTemplate(OffscreenBuffer offscreenBuffer);

    protected abstract void FinishTemplate(OffscreenBuffer offscreenBuffer, T result);

    protected abstract bool HandleEnter([NotNullWhen(true)] out T? result);

    protected virtual bool HandleTextInput(ConsoleKeyInfo keyInfo)
    {
        InputBuffer.Insert(keyInfo.KeyChar);

        return true;
    }

    protected void SetError(string errorMessage) => _formRenderer.ErrorMessage = errorMessage;

    protected void SetError(Exception exception) => SetError(exception.Message);

    protected void SetError(ValidationResult validationResult) => SetError(validationResult.ErrorMessage!);

    protected bool TryValidate([NotNullWhen(true)] object? input, IList<Func<object?, ValidationResult?>> validators)
    {
        var result = validators.Select(x => x(input))
                               .FirstOrDefault(x => x != ValidationResult.Success);

        if (result is not null)
        {
            SetError(result);

            return false;
        }

        return true;
    }

    private void CancellationHandler()
    {
        _formRenderer.Cancel();

        if (Prompt.ThrowExceptionOnCancel)
        {
            throw new PromptCanceledException(Resource.Message_PromptCanceled, GetType().Name);
        }

        Environment.Exit(1);
    }
}
