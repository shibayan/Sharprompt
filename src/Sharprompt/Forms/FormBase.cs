using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

using Sharprompt.Drivers;
using Sharprompt.Internal;
using Sharprompt.Strings;

namespace Sharprompt.Forms;

internal abstract class FormBase<T> : IDisposable
{
    protected FormBase(PromptConfiguration configuration)
    {
        _configuration = configuration;
        _consoleDriver = configuration.ConsoleDriverFactory() ?? throw new InvalidOperationException("ConsoleDriverFactory must return a non-null IConsoleDriver instance.");

        _consoleDriver.CancellationCallback = CancellationHandler;

        _formRenderer = new FormRenderer(_consoleDriver, configuration);

        KeyHandlerMaps = new()
        {
            [new ConsoleKeyBinding(ConsoleKey.Escape)] = HandleEscape
        };
    }

    private static readonly TimeSpan s_keyPollingInterval = TimeSpan.FromMilliseconds(16);

    private readonly IConsoleDriver _consoleDriver;
    private readonly FormRenderer _formRenderer;
    private readonly PromptConfiguration _configuration;

    protected PromptConfiguration Configuration => _configuration;

    protected TextInputBuffer InputBuffer { get; } = new();

    protected Dictionary<ConsoleKeyBinding, Func<bool>> KeyHandlerMaps { get; set; }

    protected int Width => _consoleDriver.WindowWidth;

    protected int Height => _consoleDriver.WindowHeight;

    public void Dispose() => _consoleDriver.Dispose();

    public T Start(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        while (true)
        {
            _formRenderer.Render(InputTemplate);

            if (!TryGetResult(cancellationToken, out var result))
            {
                continue;
            }

            _formRenderer.Render(FinishTemplate, result);

            return result;
        }
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
        foreach (var validator in validators)
        {
            var result = validator(input);

            if (result == ValidationResult.Success)
            {
                continue;
            }

            SetError(result!);

            return false;
        }

        return true;
    }

    private bool TryGetResult(CancellationToken cancellationToken, [NotNullWhen(true)] out T? result)
    {
        do
        {
            var keyInfo = ReadKey(cancellationToken);

            if (keyInfo.Key == ConsoleKey.Enter)
            {
                return HandleEnter(out result);
            }

            var binding = new ConsoleKeyBinding(keyInfo.Key, keyInfo.Modifiers);

            if (KeyHandlerMaps.TryGetValue(binding, out var keyHandler) && keyHandler())
            {
                continue;
            }

            if (!char.IsControl(keyInfo.KeyChar))
            {
                HandleTextInput(keyInfo);
            }
            else
            {
                _consoleDriver.Beep();
            }

        } while (_consoleDriver.KeyAvailable);

        result = default;

        return false;
    }

    private ConsoleKeyInfo ReadKey(CancellationToken cancellationToken)
    {
        // ReadKey blocks and cannot be interrupted, so when the token can be canceled
        // poll for available input while waiting on the token instead.
        while (true)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _formRenderer.Cancel();

                throw new OperationCanceledException(Resource.Message_PromptCanceled, cancellationToken);
            }

            if (!cancellationToken.CanBeCanceled || _consoleDriver.KeyAvailable)
            {
                return _consoleDriver.ReadKey();
            }

            cancellationToken.WaitHandle.WaitOne(s_keyPollingInterval);
        }
    }

    private void CancellationHandler()
    {
        _formRenderer.Cancel();

        if (_configuration.ThrowExceptionOnCancel)
        {
            throw new PromptCanceledException(Resource.Message_PromptCanceled, GetType().Name);
        }

        Environment.Exit(1);
    }

    private bool HandleEscape()
    {
        CancellationHandler();

        return true;
    }
}
