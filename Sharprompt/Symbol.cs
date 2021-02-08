using System;
using System.Runtime.InteropServices;

namespace Sharprompt
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

        private static bool IsUnicodeSupported => !RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || Console.OutputEncoding.CodePage == 1200 || Console.OutputEncoding.CodePage == 65001;
    }
}
