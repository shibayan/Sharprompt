using System;

using Sharprompt.Drivers;
using Sharprompt.Internal;

namespace Sharprompt.Forms;

/// <summary>
/// Renders prompt templates and transient validation errors through a reusable offscreen buffer.
/// </summary>
/// <param name="consoleDriver">The console input, output, and cursor operations used for rendering.</param>
/// <param name="configuration">The symbols, colors, console factory, and cancellation settings to use.</param>
internal class FormRenderer(IConsoleDriver consoleDriver, PromptConfiguration configuration)
{
    private readonly OffscreenBuffer _offscreenBuffer = new(consoleDriver, configuration);

    /// <summary>
    /// The validation error displayed once during the next active-prompt render.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Resets the terminal and moves output to a new line after the canceled prompt.
    /// </summary>
    public void Cancel() => _offscreenBuffer.Cancel();

    /// <summary>
    /// Renders the active prompt and displays any pending validation error once.
    /// </summary>
    /// <param name="template">Writes the prompt content into the pending output buffer.</param>
    public void Render(Action<OffscreenBuffer> template)
    {
        using (_offscreenBuffer.BeginRender())
        {
            template(_offscreenBuffer);

            if (string.IsNullOrEmpty(ErrorMessage))
            {
                return;
            }

            _offscreenBuffer.WriteLine();
            _offscreenBuffer.WriteError(ErrorMessage);

            ErrorMessage = null;
        }
    }

    /// <summary>
    /// Renders a completed-prompt template with its accepted result and ends the output line.
    /// </summary>
    /// <typeparam name="T">The result type returned by the prompt or template.</typeparam>
    /// <param name="template">Writes the prompt content into the pending output buffer.</param>
    /// <param name="result">The accepted result used to render the completed prompt.</param>
    public void Render<T>(Action<OffscreenBuffer, T> template, T result)
    {
        using (_offscreenBuffer.BeginRender())
        {
            template(_offscreenBuffer, result);

            _offscreenBuffer.WriteLine();
        }
    }
}
