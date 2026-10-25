using System;
using System.Globalization;

using Sharprompt.Drivers;
using Sharprompt.Strings;

namespace Sharprompt;

public static partial class Prompt
{
    /// <summary>
    /// The shared configuration used by all static prompt entry points.
    /// </summary>
    internal static readonly PromptConfiguration s_configuration = new();

    /// <summary>
    /// The process-wide prompt configuration. Restore settings after tests and avoid concurrent configuration changes.
    /// </summary>
    public static PromptConfiguration Configuration => s_configuration;

    /// <summary>
    /// Whether Ctrl+C and Escape throw PromptCanceledException. Defaults to false, which exits the process with code 1. Token cancellation always throws OperationCanceledException.
    /// </summary>
    public static bool ThrowExceptionOnCancel
    {
        get => s_configuration.ThrowExceptionOnCancel;
        set => s_configuration.ThrowExceptionOnCancel = value;
    }

    /// <summary>
    /// Creates an IConsoleDriver for each prompt. The factory and its return value must not be null. Each prompt disposes its driver; restore this process-wide setting after tests.
    /// </summary>
    public static Func<IConsoleDriver> ConsoleDriverFactory
    {
        get => s_configuration.ConsoleDriverFactory;
        set => s_configuration.ConsoleDriverFactory = value;
    }

    /// <summary>
    /// The culture for localized messages. Does not change input conversion, which always uses the invariant culture.
    /// </summary>
    public static CultureInfo Culture
    {
        get => Resource.Culture;
        set => Resource.Culture = value;
    }

    /// <summary>
    /// The process-wide colors used when rendering prompts.
    /// </summary>
    public static PromptColorSchema ColorSchema => s_configuration.ColorSchema;

    /// <summary>
    /// The process-wide symbols used when rendering prompts.
    /// </summary>
    public static PromptSymbols Symbols => s_configuration.Symbols;
}
