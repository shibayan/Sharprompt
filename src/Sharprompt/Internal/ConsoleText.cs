using System.Collections.Generic;
using System.Text;

namespace Sharprompt.Internal;

// Prompts show text from many sources: messages, items, answers, default values and error messages,
// any of which can contain control characters. OffscreenBuffer normalizes text here before measuring
// and writing it, so the width it counts is the width the terminal shows:
// - Line breaks (CRLF, LF, CR, NEL, LS and PS) end the line, and the next text starts a new one.
// - Tabs are expanded with spaces to the next multiple of 8 columns from the start of the line.
// - SGR sequences (ESC [ ... m), which only set colors and styles, are kept and take no width. Each line is
//   written separately, and the console driver resets colors after every write, so a style ends with its line.
//   Other escape sequences, which can move the cursor or change the terminal's state, are removed.
// - Other control characters are shown as visible symbols: C0 controls and DEL as their Control
//   Pictures (␀ to ␟ and ␡), and C1 controls as U+FFFD.
internal static class ConsoleText
{
    private const char Escape = '\x1b';
    private const char LineSeparator = (char)0x2028;
    private const char ParagraphSeparator = (char)0x2029;
    private const int TabSize = 8;

    // Splits text into normalized lines. startColumn is the width already written on the current line,
    // which tab expansion counts from.
    public static IReadOnlyList<string> Normalize(string text, int startColumn)
    {
        if (!NeedsNormalization(text))
        {
            return [text];
        }

        var lines = new List<string>();
        var builder = new StringBuilder(text.Length);

        // The display column is accumulated as the line grows: a tab measures only the text appended since the
        // previous tab, which is safe because a control character always ends a grapheme cluster.
        var column = startColumn;
        var measuredLength = 0;

        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];

            switch (c)
            {
                case '\r' or '\n' or '\u0085' or LineSeparator or ParagraphSeparator:
                    if (c == '\r' && i + 1 < text.Length && text[i + 1] == '\n')
                    {
                        i++;
                    }

                    lines.Add(builder.ToString());
                    builder.Clear();

                    column = 0;
                    measuredLength = 0;
                    break;

                case '\t':
                    column += MeasureWidth(builder.ToString(measuredLength, builder.Length - measuredLength));

                    var spaces = TabSize - (column % TabSize);

                    builder.Append(' ', spaces);

                    column += spaces;
                    measuredLength = builder.Length;
                    break;

                case Escape when TryReadCsi(text, i, out var end, out var isSgr) && isSgr:
                    builder.Append(text, i, end - i + 1);

                    i = end;
                    break;

                case Escape:
                    i = AppendEscapeSequence(text, i, builder);
                    break;

                default:
                    builder.Append(char.IsControl(c) ? ToVisibleSymbol(c) : c);
                    break;
            }
        }

        lines.Add(builder.ToString());

        return lines;
    }

    // Measures normalized text, in which the only escape sequences left are SGR sequences.
    public static int MeasureWidth(string text)
    {
        if (!text.Contains(Escape))
        {
            return text.GetWidth();
        }

        var builder = new StringBuilder(text.Length);

        for (var i = 0; i < text.Length; i++)
        {
            if (text[i] == Escape && TryReadCsi(text, i, out var end, out _))
            {
                i = end;
                continue;
            }

            builder.Append(text[i]);
        }

        return builder.ToString().GetWidth();
    }

    private static bool NeedsNormalization(string text)
    {
        foreach (var c in text)
        {
            if (char.IsControl(c) || c is LineSeparator or ParagraphSeparator)
            {
                return true;
            }
        }

        return false;
    }

    // Returns the index of the last character consumed. SGR sequences are handled by the caller.
    private static int AppendEscapeSequence(string text, int start, StringBuilder builder)
    {
        if (TryReadCsi(text, start, out var end, out _))
        {
            return end;
        }

        var next = start + 1 < text.Length ? text[start + 1] : '\0';

        switch (next)
        {
            // An unterminated CSI is dropped up to the first character that cannot be part of it.
            case '[':
                var index = start + 2;

                while (index < text.Length && text[index] is >= '\x20' and <= '\x3f')
                {
                    index++;
                }

                return index - 1;

            // OSC, DCS, SOS, PM and APC strings run until BEL or ST (ESC \), or to the end of the text.
            case ']' or 'P' or 'X' or '^' or '_':
                for (var i = start + 2; i < text.Length; i++)
                {
                    if (text[i] == '\a')
                    {
                        return i;
                    }

                    if (text[i] == Escape && i + 1 < text.Length && text[i + 1] == '\\')
                    {
                        return i + 1;
                    }
                }

                return text.Length - 1;

            // Other sequences are ESC, any intermediate bytes, and a final byte.
            case >= '\x20' and <= '\x7e':
                var final = start + 1;

                while (final < text.Length && text[final] is >= '\x20' and <= '\x2f')
                {
                    final++;
                }

                if (final < text.Length && text[final] is >= '\x30' and <= '\x7e')
                {
                    return final;
                }

                builder.Append(ToVisibleSymbol(Escape));

                return start;

            default:
                builder.Append(ToVisibleSymbol(Escape));

                return start;
        }
    }

    // CSI: ESC [, parameter bytes (0x30 to 0x3F), intermediate bytes (0x20 to 0x2F) and a final byte (0x40 to 0x7E).
    // It is SGR when the final byte is m and the parameters are only digits, semicolons and colons. Private
    // parameters (<, =, >, ?) make it a different command, such as ESC[>4;2m for xterm's modifyOtherKeys.
    private static bool TryReadCsi(string text, int start, out int end, out bool isSgr)
    {
        end = start;
        isSgr = false;

        if (start + 1 >= text.Length || text[start + 1] != '[')
        {
            return false;
        }

        var index = start + 2;
        var hasPrivateParameters = false;

        while (index < text.Length && text[index] is >= '\x30' and <= '\x3f')
        {
            hasPrivateParameters |= text[index] is >= '\x3c' and <= '\x3f';
            index++;
        }

        var parametersEnd = index;

        while (index < text.Length && text[index] is >= '\x20' and <= '\x2f')
        {
            index++;
        }

        if (index >= text.Length || text[index] is not (>= '\x40' and <= '\x7e'))
        {
            return false;
        }

        end = index;
        isSgr = text[index] == 'm' && parametersEnd == index && !hasPrivateParameters;

        return true;
    }

    private static char ToVisibleSymbol(char c) => c switch
    {
        <= '\x1f' => (char)(0x2400 + c),
        '\x7f' => (char)0x2421,
        _ => (char)0xfffd
    };
}
