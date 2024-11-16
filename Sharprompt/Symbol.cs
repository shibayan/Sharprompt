using System;
using System.Runtime.InteropServices;

namespace Sharprompt;

public class Symbol(string value, string fallbackValue)
{
    public override string ToString() => IsUnicodeSupported ? value : fallbackValue;

    public static implicit operator string(Symbol symbol) => symbol.ToString();

    private static bool IsUnicodeSupported => !RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || Console.OutputEncoding.CodePage is 1200 or 65001;
}
