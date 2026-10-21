using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Sharprompt.Internal;

// Prompts show text from many sources: messages, items, answers, default values and error messages,
// any of which can contain control characters. OffscreenBuffer normalizes text here before measuring
// and writing it, so the width it counts is the width the terminal shows:
// - Line breaks (CRLF, LF, CR, NEL, LS and PS) end the line, and the next text starts a new one.
// - Tabs are expanded with spaces to the next multiple of 8 columns from the start of the line.
// - SGR sequences (ESC [ ... m), which only set colors and styles, are kept and take no width.
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

        // Each line is written separately, and the console driver resets colors after every write, so the
        // effective SGR state at a line break is restored at the start of the next line.
        var sgrState = new SgrState();

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
                    sgrState.AppendTo(builder);

                    column = 0;
                    measuredLength = builder.Length;
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
                    sgrState.Apply(text.AsSpan(i + 2, end - i - 2));

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

    // The effective SGR state, kept per known attribute, so a continuation line restores it with one compact
    // sequence however many changes came before. Resets of an attribute (22, 23, 54, ...) clear it, except
    // that the default colors (39, 49) and the full reset (0) are kept, since they override the driver's color.
    private sealed class SgrState
    {
        // Each attribute is stored under one slot, and its resets clear that slot. The slots are ordered so
        // that the full reset comes first.
        private const int ResetSlot = 0;
        private const int ItalicSlot = 3;
        private const int FontSlot = 10;
        private const int ProportionalSpacingSlot = 26;
        private const int ForegroundSlot = 30;
        private const int BackgroundSlot = 40;
        private const int FrameSlot = 51;
        private const int UnderlineColorSlot = 58;
        private const int IdeogramSlot = 60;
        private const int ScriptSlot = 73;

        // Slot to the parameter that sets it; sorted, so the restoring sequence is deterministic.
        private readonly SortedDictionary<int, string> _attributes = [];

        public void Apply(ReadOnlySpan<char> parameters)
        {
            var tokens = parameters.ToString().Split(';');

            for (var i = 0; i < tokens.Length; i++)
            {
                // Parameters are parsed as numbers and stored in canonical form, so zero padding neither changes
                // their meaning nor makes the restoring sequence longer than the attributes it sets.
                var parts = tokens[i].Split(':');

                if (!TryParseParameter(parts[0], out var code))
                {
                    continue;
                }

                string token;

                if (code is 38 or 48 or 58)
                {
                    if (!TryReadExtendedColor(code, tokens, parts, ref i, out token))
                    {
                        continue;
                    }
                }
                else if (code == 4 && parts.Length > 1 && TryParseParameter(parts[1], out var underlineStyle))
                {
                    token = "4:" + Format(underlineStyle);
                }
                else
                {
                    token = Format(code);
                }

                switch (code)
                {
                    // The console driver applies its own foreground color to every line, so a reset to the
                    // terminal defaults is kept as state of its own and restored first.
                    case 0:
                        _attributes.Clear();
                        _attributes[ResetSlot] = "0";
                        break;
                    case 1 or 2 or 4 or 5 or 7 or 8 or 9 or 53:
                        _attributes[code] = token;
                        break;
                    case 3 or 20:
                        _attributes[ItalicSlot] = token;
                        break;
                    case 6:
                        _attributes[5] = token;
                        break;
                    case 21:
                        _attributes[4] = token;
                        break;
                    case 22:
                        _attributes.Remove(1);
                        _attributes.Remove(2);
                        break;
                    case 23 or 24 or 25 or 27 or 28 or 29:
                        _attributes.Remove(code - 20);
                        break;
                    case 55:
                        _attributes.Remove(53);
                        break;
                    case 10:
                        _attributes.Remove(FontSlot);
                        break;
                    case >= 11 and <= 19:
                        _attributes[FontSlot] = token;
                        break;
                    case 26:
                        _attributes[ProportionalSpacingSlot] = token;
                        break;
                    case 50:
                        _attributes.Remove(ProportionalSpacingSlot);
                        break;
                    // The default colors are kept too (39 and 49), for the same reason as a full reset.
                    case (>= 30 and <= 39) or (>= 90 and <= 97):
                        _attributes[ForegroundSlot] = token;
                        break;
                    case (>= 40 and <= 49) or (>= 100 and <= 107):
                        _attributes[BackgroundSlot] = token;
                        break;
                    case 51 or 52:
                        _attributes[FrameSlot] = token;
                        break;
                    case 54:
                        _attributes.Remove(FrameSlot);
                        break;
                    case 58:
                        _attributes[UnderlineColorSlot] = token;
                        break;
                    case 59:
                        _attributes.Remove(UnderlineColorSlot);
                        break;
                    case >= 60 and <= 64:
                        _attributes[IdeogramSlot] = token;
                        break;
                    case 65:
                        _attributes.Remove(IdeogramSlot);
                        break;
                    case 73 or 74:
                        _attributes[ScriptSlot] = token;
                        break;
                    case 75:
                        _attributes.Remove(ScriptSlot);
                        break;

                        // Unknown codes are written where they appear but not carried over, so the restoring
                        // sequence stays bounded by the known attributes.
                }
            }
        }

        // Extended colors: 38;5;n or 38;2;r;g;b take the following parameters, and 38:5:n or 38:2:[space]:r:g:b
        // carry them as sub-parameters. Any parameter that is not a number leaves the color out of the state.
        private static bool TryReadExtendedColor(int code, string[] tokens, string[] parts, ref int index, out string token)
        {
            token = "";

            if (parts.Length > 1)
            {
                if (!TryParseParameter(parts[1], out var colonMode))
                {
                    return false;
                }

                if (colonMode == 5 && parts.Length >= 3 && TryParseParameter(parts[2], out var colorIndex))
                {
                    token = Format(code) + ":5:" + Format(colorIndex);
                    return true;
                }

                if (colonMode == 2 && parts.Length >= 5 && TryParseRgb(parts[^3], parts[^2], parts[^1], out var colonRgb))
                {
                    token = Format(code) + ":2::" + colonRgb.Replace(';', ':');
                    return true;
                }

                return false;
            }

            if (index + 1 >= tokens.Length || !TryParseParameter(tokens[index + 1], out var mode))
            {
                return false;
            }

            var count = mode switch { 5 => 2, 2 => 4, _ => 0 };

            if (count == 0)
            {
                return false;
            }

            var available = Math.Min(count, tokens.Length - 1 - index);
            var start = index;

            index += available;

            if (available < count)
            {
                return false;
            }

            if (mode == 5 && TryParseParameter(tokens[start + 2], out var semicolonIndex))
            {
                token = Format(code) + ";5;" + Format(semicolonIndex);
                return true;
            }

            if (mode == 2 && TryParseRgb(tokens[start + 2], tokens[start + 3], tokens[start + 4], out var semicolonRgb))
            {
                token = Format(code) + ";2;" + semicolonRgb;
                return true;
            }

            return false;
        }

        private static bool TryParseRgb(string red, string green, string blue, out string rgb)
        {
            rgb = "";

            if (!TryParseParameter(red, out var r) || !TryParseParameter(green, out var g) || !TryParseParameter(blue, out var b))
            {
                return false;
            }

            rgb = Format(r) + ";" + Format(g) + ";" + Format(b);

            return true;
        }

        // An empty parameter means 0.
        private static bool TryParseParameter(string text, out int value)
        {
            if (text.Length == 0)
            {
                value = 0;
                return true;
            }

            return int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out value);
        }

        private static string Format(int value) => value.ToString(CultureInfo.InvariantCulture);

        public void AppendTo(StringBuilder builder)
        {
            if (_attributes.Count == 0)
            {
                return;
            }

            builder.Append(Escape).Append('[').AppendJoin(';', _attributes.Values).Append('m');
        }
    }

    private static char ToVisibleSymbol(char c) => c switch
    {
        <= '\x1f' => (char)(0x2400 + c),
        '\x7f' => (char)0x2421,
        _ => (char)0xfffd
    };
}
