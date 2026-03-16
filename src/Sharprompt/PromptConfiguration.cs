using System;

using Sharprompt.Drivers;

namespace Sharprompt;

public class PromptConfiguration
{
    public bool ThrowExceptionOnCancel { get; set; }

    private Func<IConsoleDriver> _consoleDriverFactory = () => new DefaultConsoleDriver();

    public Func<IConsoleDriver> ConsoleDriverFactory
    {
        get => _consoleDriverFactory;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            _consoleDriverFactory = value;
        }
    }

    public PromptColorSchema ColorSchema { get; } = new();

    public PromptSymbols Symbols { get; } = new();
}

public class PromptColorSchema
{
    public ConsoleColor DoneSymbol { get; set; } = ConsoleColor.Green;
    public ConsoleColor PromptSymbol { get; set; } = ConsoleColor.Green;
    public ConsoleColor Answer { get; set; } = ConsoleColor.Cyan;
    public ConsoleColor Select { get; set; } = ConsoleColor.Green;
    public ConsoleColor Error { get; set; } = ConsoleColor.Red;
    public ConsoleColor Hint { get; set; } = ConsoleColor.DarkGray;
    public ConsoleColor DisabledOption { get; set; } = ConsoleColor.DarkCyan;
}

public class PromptSymbols
{
    public Symbol Prompt { get; set; } = new("?", "?");
    public Symbol Done { get; set; } = new("✔", "V");
    public Symbol Error { get; set; } = new("»", ">>");
    public Symbol Selector { get; set; } = new("›", ">");
    public Symbol Selected { get; set; } = new("◉", "(*)");
    public Symbol NotSelect { get; set; } = new("◯", "( )");
}
