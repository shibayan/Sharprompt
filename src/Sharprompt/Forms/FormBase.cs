using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

using Sharprompt.Drivers;
using Sharprompt.Internal;
using Sharprompt.Strings;

namespace Sharprompt.Forms;

/// <summary>
/// Owns the console driver and runs the rendering, input dispatch, validation, and cancellation lifecycle of a prompt.
/// </summary>
/// <typeparam name="T">The result type returned by the prompt or template.</typeparam>
internal abstract class FormBase<T> : IDisposable
{
    /// <summary>
    /// Creates and owns a console driver, initializes rendering, and registers Escape cancellation.
    /// </summary>
    /// <param name="configuration">The symbols, colors, console factory, and cancellation settings to use.</param>
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

    private readonly IConsoleDriver _consoleDriver;
    private readonly FormRenderer _formRenderer;
    private readonly PromptConfiguration _configuration;

    /// <summary>
    /// The configuration captured for this prompt, including colors, symbols, and cancellation behavior.
    /// </summary>
    protected PromptConfiguration Configuration => _configuration;

    /// <summary>
    /// The text and edit cursor used by input handlers and templates.
    /// </summary>
    protected TextInputBuffer InputBuffer { get; } = new();

    /// <summary>
    /// Maps key and modifier combinations to handlers that report whether they handled the key.
    /// </summary>
    protected Dictionary<ConsoleKeyBinding, Func<bool>> KeyHandlerMaps { get; set; }

    /// <summary>
    /// The current visible terminal width in columns.
    /// </summary>
    protected int Width => _consoleDriver.WindowWidth;

    /// <summary>
    /// The current visible terminal height in rows.
    /// </summary>
    protected int Height => _consoleDriver.WindowHeight;

    /// <summary>
    /// Disposes the console driver owned by this prompt.
    /// </summary>
    public void Dispose() => _consoleDriver.Dispose();

    /// <summary>
    /// Renders and dispatches input until a result is accepted, then renders the completed prompt.
    /// </summary>
    /// <param name="cancellationToken">The token observed before prompting and while waiting for keys.</param>
    /// <returns>The accepted prompt result.</returns>
    /// <exception cref="OperationCanceledException">The cancellation token is canceled.</exception>
    /// <exception cref="PromptCanceledException">The user cancels and exception-based user cancellation is enabled.</exception>
    /// <remarks>User cancellation exits the process when exception-based cancellation is disabled.</remarks>
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

    /// <summary>
    /// Writes the active prompt, current input, and any supporting options to the pending buffer.
    /// </summary>
    /// <param name="offscreenBuffer">The pending output buffer to write or render.</param>
    protected abstract void InputTemplate(OffscreenBuffer offscreenBuffer);

    /// <summary>
    /// Writes the completed prompt and the representation of its accepted result.
    /// </summary>
    /// <param name="offscreenBuffer">The pending output buffer to write or render.</param>
    /// <param name="result">The accepted result used to render the completed prompt.</param>
    protected abstract void FinishTemplate(OffscreenBuffer offscreenBuffer, T result);

    /// <summary>
    /// Attempts to accept the current input, retaining the prompt when validation or selection is incomplete.
    /// </summary>
    /// <param name="result">Receives the accepted result when the method returns true; otherwise it must not be consumed.</param>
    /// <returns>True when the prompt can finish with the result; otherwise false to continue prompting.</returns>
    protected abstract bool HandleEnter([NotNullWhen(true)] out T? result);

    /// <summary>
    /// Inserts the typed character at the edit cursor.
    /// </summary>
    /// <param name="keyInfo">The character and key information supplied by the console driver.</param>
    /// <returns>True when the key changed or handled the input; otherwise false.</returns>
    protected virtual bool HandleTextInput(ConsoleKeyInfo keyInfo)
    {
        InputBuffer.Insert(keyInfo.KeyChar);

        return true;
    }

    /// <summary>
    /// Sets the validation message to display during the next render.
    /// </summary>
    /// <param name="errorMessage">The error text to display on the next render.</param>
    protected void SetError(string errorMessage) => _formRenderer.ErrorMessage = errorMessage;

    /// <summary>
    /// Sets the validation message to display during the next render.
    /// </summary>
    /// <param name="exception">The exception whose message is displayed as an input error.</param>
    protected void SetError(Exception exception) => SetError(exception.Message);

    /// <summary>
    /// Sets the validation message to display during the next render.
    /// </summary>
    /// <param name="validationResult">The failed validation result containing the error message.</param>
    protected void SetError(ValidationResult validationResult) => SetError(validationResult.ErrorMessage!);

    /// <summary>
    /// Runs validators in order, stopping at the first failure and scheduling its error for display.
    /// </summary>
    /// <param name="input">The converted input value passed to each validator.</param>
    /// <param name="validators">The validators to apply or append in their existing order.</param>
    /// <returns>True when all validators accept the value; otherwise false after scheduling the first error.</returns>
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
        try
        {
            return _consoleDriver.ReadKey(cancellationToken);
        }
        catch (OperationCanceledException ex) when (ex.CancellationToken == cancellationToken)
        {
            _formRenderer.Cancel();

            throw new OperationCanceledException(Resource.Message_PromptCanceled, ex, cancellationToken);
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
