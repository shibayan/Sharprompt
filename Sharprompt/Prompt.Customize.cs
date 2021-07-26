using System;

namespace Sharprompt
{
    public static partial class Prompt
    {
        public const int DefaultIdleReadKey = 30;

        public static class Messages
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
            public static char YesKey { get; set; } = 'Y';
            public static string LongYesKey { get; set; } = "Yes";
            public static char NoKey { get; set; } = 'N';
            public static string LongNoKey { get; set; } = "No";
            public static string AnyKey { get; set; } = "Press any key";
            public static string Invalid { get; set; } = "Value is invalid";
            public static string Required { get; set; } = "Value is required";
            public static string MinLength { get; set; } = "Value is too short";
            public static string MaxLength { get; set; } = "Value is too long";
            public static string NoMatchRegex { get; set; } = "Value is not match pattern";
            public static char PasswordChar { get; set; } = '*';
            public static string ConfirmKeyNavigation { get; set; } = "Hit enter to finish";
            public static string MultiSelectMinSelection { get; set; } = "A minimum selection of {0} items is required";
            public static string MultiSelectMaxSelection { get; set; } = "A maximum selection of {0} items is required";
            public static string ListMinSelection { get; set; } = "A minimum input of {0} items is required";
            public static string ListMaxSelection { get; set; } = "A maximum input of {0} items is required";
            public static string ListKeyNavigation { get; set; } = "Ctrl + Delete to remove, hit enter to finish";
            public static string SelectKeyNavigation { get; set; } = "Hit enter to select";
            public static string KeyNavPaging { get; set; } = "LeftArrow/RightArrow to paging, ";
            public static string MultiSelectKeyNavigation { get; set; } = "Hit space to select, hit enter to finish";
            public static string PaginationTemplate { get; set; } = "  {0} items, {1}/{2} pages";
            public static string FolderKeyNavigation { get; set; } = "Ctrl + LeftArrow/RightArrow to sub-folder, Hit enter to select";
            public static string FolderCurrentPath { get; set; } = "Current Folder :";
            public static string FileNotSelected { get; set; } = "Selected not a file!";
        }

        public static class ColorSchema
        {
            public static ConsoleColor PaginationInfo { get; set; } = ConsoleColor.DarkGray;
            public static ConsoleColor KeyNavigation { get; set; } = ConsoleColor.DarkGray;
            public static ConsoleColor Answer { get; set; } = ConsoleColor.Cyan;
            public static ConsoleColor Select { get; set; } = ConsoleColor.Green;
            public static ConsoleColor DisabledOption { get; set; } = ConsoleColor.DarkCyan;
        }

        public static class Symbols
        {
            public static Symbol File { get; set; } = new Symbol("■", "-");
            public static Symbol Folder { get; set; } = new Symbol("►", ">");
            public static Symbol Prompt { get; set; } = new Symbol("?", "?");
            public static Symbol Done { get; set; } = new Symbol("✔", "V");
            public static Symbol Error { get; set; } = new Symbol("»", ">>");
            public static Symbol Selector { get; set; } = new Symbol("›", ">");
            public static Symbol Selected { get; set; } = new Symbol("◉", "(*)");
            public static Symbol NotSelect { get; set; } = new Symbol("◯", "( )");
        }
    }
}
