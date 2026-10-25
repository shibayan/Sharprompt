using System;
using System.Runtime.InteropServices;

namespace Sharprompt;

/// <summary>
/// A prompt symbol with a fallback for Windows terminals whose output encoding is not Unicode.
/// </summary>
/// <param name="value">The Unicode symbol.</param>
/// <param name="fallbackValue">The fallback symbol.</param>
public class Symbol(string value, string fallbackValue)
{
    /// <summary>
    /// Returns the Unicode symbol or its fallback for the current output encoding.
    /// </summary>
    /// <returns>The symbol text.</returns>
    public override string ToString() => IsUnicodeSupported ? value : fallbackValue;

    /// <summary>
    /// Converts a symbol to the text appropriate for the current output encoding.
    /// </summary>
    /// <param name="symbol">The symbol to format.</param>
    /// <returns>The symbol text.</returns>
    public static implicit operator string(Symbol symbol) => symbol.ToString();

    private static bool IsUnicodeSupported => !RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || Console.OutputEncoding.CodePage is 1200 or 65001;
}
