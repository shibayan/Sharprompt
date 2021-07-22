using System;

namespace Sharprompt
{
    public static partial class Prompt
    {
        public const int DefaultIdleReadKey = 30;

        public static class DefaultMessageValues
        {
            private static int _IdleReadKey = DefaultIdleReadKey;
            public static int IdleReadKey
            {
                get
                {
                    if (_IdleReadKey == 0)
                    {
                        return DefaultIdleReadKey;
                    }
                    return _IdleReadKey;
                }
                set
                {
                    if (value < 10)
                    {
                        _IdleReadKey = 10;
                    }
                    else if (value > 100)
                    {
                        _IdleReadKey = 100;
                    }
                    else
                    {
                        _IdleReadKey = value;
                    }
                }
            }
            public static char DefaultYesKey { get; set; } = 'Y';
            public static char DefaultNoKey { get; set; } = 'N';
            public static string DefaultAnyKeyMessage { get; set; } = "Press any key";
            public static string DefaultInvalidValueMessage { get; set; } = "Value is invalid";
            public static string DefaultRequiredMessage { get; set; } = "Value is required";
            public static string DefaultMinLengthMessage { get; set; } = "Value is too short";
            public static string DefauMaxLengthhMessage { get; set; } = "Value is too long";
            public static string DefaultRegularExpressionMessage { get; set; } = "Value is not match pattern";
            public static char DefautPasswordChar { get; set; } = '*';
            public static string DefaultMultiSelectMinSelectionMessage { get; set; } = "A minimum selection of {0} items is required";
            public static string DefaultMultiSelectInputTemplateMessage { get; set; } = "(Hit space to select) ";

        }

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
