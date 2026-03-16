using System;
using System.Globalization;

using Sharprompt.Drivers;
using Sharprompt.Strings;

namespace Sharprompt;

public static partial class Prompt
{
    internal static readonly PromptConfiguration s_configuration = new();

    public static PromptConfiguration Configuration => s_configuration;

    public static bool ThrowExceptionOnCancel
    {
        get => s_configuration.ThrowExceptionOnCancel;
        set => s_configuration.ThrowExceptionOnCancel = value;
    }

    public static Func<IConsoleDriver> ConsoleDriverFactory
    {
        get => s_configuration.ConsoleDriverFactory;
        set => s_configuration.ConsoleDriverFactory = value;
    }

    public static CultureInfo Culture
    {
        get => Resource.Culture;
        set => Resource.Culture = value;
    }

    public static PromptColorSchema ColorSchema => s_configuration.ColorSchema;

    public static PromptSymbols Symbols => s_configuration.Symbols;
}
