using System;

namespace Sharprompt
{
    public static partial class Prompt
    {
        public static class ColorSchema
        {
            public static ConsoleColor Answer { get; set; } = ConsoleColor.Cyan;
            public static ConsoleColor Select { get; set; } = ConsoleColor.Green;
            public static ConsoleColor DisabledOption { get; set; } = ConsoleColor.DarkCyan;
        }

        public static class Symbols
        {
            public static Symbol Prompt { get; set; } = new Symbol("?", "?");
            public static Symbol Done { get; set; } = new Symbol("✔", "V");
            public static Symbol Error { get; set; } = new Symbol("»", ">>");
            public static Symbol Selector { get; set; } = new Symbol("›", ">");
            public static Symbol Selected { get; set; } = new Symbol("◉", "(*)");
            public static Symbol NotSelect { get; set; } = new Symbol("◯", "( )");
        }
    }
}
