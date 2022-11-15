using System;
using System.Globalization;

using Sharprompt.Strings;

namespace Sharprompt;

public static partial class Prompt
{
    public static bool ThrowExceptionOnCancel { get; set; } = false;

    public static CultureInfo Culture
    {
        get => Resource.Culture;
        set => Resource.Culture = value;
    }

    public static class ColorSchema
    {
        public static ConsoleColor DoneSymbol { get; set; } = ConsoleColor.Green;
        public static ConsoleColor PromptSymbol { get; set; } = ConsoleColor.Green;
        public static ConsoleColor Answer { get; set; } = ConsoleColor.Cyan;
        public static ConsoleColor Select { get; set; } = ConsoleColor.Green;
        public static ConsoleColor Error { get; set; } = ConsoleColor.Red;
        public static ConsoleColor Hint { get; set; } = ConsoleColor.DarkGray;
        public static ConsoleColor DisabledOption { get; set; } = ConsoleColor.DarkCyan;
    }

    public static class Symbols
    {
        public static Symbol Prompt { get; set; } = new("?", "?");
        public static Symbol Done { get; set; } = new("✔", "V");
        public static Symbol Error { get; set; } = new("»", ">>");
        public static Symbol Selector { get; set; } = new("›", ">");
        public static Symbol Selected { get; set; } = new("◉", "(*)");
        public static Symbol NotSelect { get; set; } = new("◯", "( )");
    }
}
