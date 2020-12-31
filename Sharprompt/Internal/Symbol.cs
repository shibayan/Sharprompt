using System;
using System.Runtime.InteropServices;

namespace Sharprompt.Internal
{
    public class Symbol
    {
        public Symbol(string value, string fallbackValue)
        {
            _value = value;
            _fallbackValue = fallbackValue;
        }

        private readonly string _value;
        private readonly string _fallbackValue;

        public override string ToString()
        {
            return IsUnicodeSupported ? _value : _fallbackValue;
        }

        public static implicit operator string(Symbol symbol) => symbol.ToString();

        public static Symbol Prompt { get; set; } = new Symbol("?", "?");
        public static Symbol Done { get; set; } = new Symbol("✔", "V");
        public static Symbol Error { get; set; } = new Symbol("»", ">>");
        public static Symbol Selector { get; set; } = new Symbol("›", ">");
        public static Symbol Selected { get; set; } = new Symbol("◉", "(*)");
        public static Symbol NotSelect { get; set; } = new Symbol("◯", "( )");

        private static bool IsUnicodeSupported => !RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || (Console.OutputEncoding.CodePage == 1200 || Console.OutputEncoding.CodePage == 65001);
    }
}
