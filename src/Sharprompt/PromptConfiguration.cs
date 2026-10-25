using System;

using Sharprompt.Drivers;

namespace Sharprompt;

/// <summary>
/// Process-wide settings for interactive prompts.
/// </summary>
public class PromptConfiguration
{
    /// <summary>
    /// Whether Ctrl+C and Escape throw PromptCanceledException. Defaults to false, which exits the process with code 1. Token cancellation always throws OperationCanceledException.
    /// </summary>
    public bool ThrowExceptionOnCancel { get; set; }

    private Func<IConsoleDriver> _consoleDriverFactory = () => new DefaultConsoleDriver();

    /// <summary>
    /// Creates an IConsoleDriver for each prompt. The factory and its return value must not be null. Each prompt disposes its driver; restore this process-wide setting after tests.
    /// </summary>
    public Func<IConsoleDriver> ConsoleDriverFactory
    {
        get => _consoleDriverFactory;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            _consoleDriverFactory = value;
        }
    }

    /// <summary>
    /// The process-wide colors used when rendering prompts.
    /// </summary>
    public PromptColorSchema ColorSchema { get; } = new();

    /// <summary>
    /// The process-wide symbols used when rendering prompts.
    /// </summary>
    public PromptSymbols Symbols { get; } = new();
}

/// <summary>
/// Colors used by the prompt renderer.
/// </summary>
public class PromptColorSchema
{
    /// <summary>
    /// The completed-prompt symbol color. Defaults to Green.
    /// </summary>
    public ConsoleColor DoneSymbol { get; set; } = ConsoleColor.Green;

    /// <summary>
    /// The active-prompt symbol color. Defaults to Green.
    /// </summary>
    public ConsoleColor PromptSymbol { get; set; } = ConsoleColor.Green;

    /// <summary>
    /// The answer color. Defaults to Cyan.
    /// </summary>
    public ConsoleColor Answer { get; set; } = ConsoleColor.Cyan;

    /// <summary>
    /// The highlighted-item color. Defaults to Green.
    /// </summary>
    public ConsoleColor Select { get; set; } = ConsoleColor.Green;

    /// <summary>
    /// The validation-error color. Defaults to Red.
    /// </summary>
    public ConsoleColor Error { get; set; } = ConsoleColor.Red;

    /// <summary>
    /// The hint color. Defaults to DarkGray.
    /// </summary>
    public ConsoleColor Hint { get; set; } = ConsoleColor.DarkGray;

    /// <summary>
    /// The disabled-option color. Defaults to DarkCyan.
    /// </summary>
    public ConsoleColor DisabledOption { get; set; } = ConsoleColor.DarkCyan;
}

/// <summary>
/// Symbols used by the prompt renderer.
/// </summary>
public class PromptSymbols
{
    /// <summary>
    /// The active-prompt symbol. Defaults to "?".
    /// </summary>
    public Symbol Prompt { get; set; } = new("?", "?");

    /// <summary>
    /// The completed-prompt symbol. Defaults to "✔", with "V" as its fallback.
    /// </summary>
    public Symbol Done { get; set; } = new("✔", "V");

    /// <summary>
    /// The error symbol. Defaults to "»", with ">>" as its fallback.
    /// </summary>
    public Symbol Error { get; set; } = new("»", ">>");

    /// <summary>
    /// The highlighted-item marker. Defaults to "›", with ">" as its fallback.
    /// </summary>
    public Symbol Selector { get; set; } = new("›", ">");

    /// <summary>
    /// The selected-item marker. Defaults to "◉", with "(*)" as its fallback.
    /// </summary>
    public Symbol Selected { get; set; } = new("◉", "(*)");

    /// <summary>
    /// The unselected-item marker. Defaults to "◯", with "( )" as its fallback.
    /// </summary>
    public Symbol NotSelect { get; set; } = new("◯", "( )");
}
