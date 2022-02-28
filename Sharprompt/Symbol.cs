using System;
using System.Runtime.InteropServices;

using Sharprompt.Drivers;

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

        public override string ToString() => IsUnicodeSupported ? _value : _fallbackValue;

        public static implicit operator string(Symbol symbol) => symbol.ToString();

        private static bool IsUnicodeSupported
        {
            get
            {
                var consoleDriver = ConsoleDriverFactory.Instance.Create();
                return consoleDriver.IsUnicodeSupported;
            }
        }
    }
}
