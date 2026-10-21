using System.Linq;

using Sharprompt.Internal;

using Xunit;

namespace Sharprompt.Tests;

public class ConsoleTextTests
{
    [Theory]
    [InlineData("plain", new[] { "plain" })]
    [InlineData("a\nb", new[] { "a", "b" })]
    [InlineData("a\r\nb", new[] { "a", "b" })]
    [InlineData("a\rb", new[] { "a", "b" })]
    [InlineData("a\n\nb", new[] { "a", "", "b" })]
    [InlineData("a\u0085b\u2028c\u2029d", new[] { "a", "b", "c", "d" })]
    [InlineData("a\n", new[] { "a", "" })]
    public void Normalize_LineBreaks_SplitLines(string text, string[] expected)
    {
        Assert.Equal(expected, ConsoleText.Normalize(text, 0));
    }

    [Theory]
    [InlineData("\tx", 0, "        x")]
    [InlineData("ab\tx", 0, "ab      x")]
    [InlineData("\tx", 3, "     x")]
    [InlineData("あ\tx", 0, "あ      x")]
    [InlineData("12345678\tx", 0, "12345678        x")]
    public void Normalize_Tabs_ExpandToNextTabStop(string text, int startColumn, string expected)
    {
        Assert.Equal([expected], ConsoleText.Normalize(text, startColumn));
    }

    [Fact]
    public void Normalize_TabAfterLineBreak_CountsFromNewLine()
    {
        Assert.Equal(["ab", "        x"], ConsoleText.Normalize("ab\n\tx", 5));
    }

    [Theory]
    [InlineData("\x1b[31mred\x1b[0m", "\x1b[31mred\x1b[0m", 3)]
    [InlineData("\x1b[1;38;5;208mbold\x1b[m", "\x1b[1;38;5;208mbold\x1b[m", 4)]
    [InlineData("\x1b[2Jclear", "clear", 5)]
    [InlineData("\x1b[10;20Hmove", "move", 4)]
    [InlineData("\x1b[?25lhidden", "hidden", 6)]
    [InlineData("\x1b[>4;2mkeys", "keys", 4)]
    [InlineData("\x1b[<0;1;1mmouse", "mouse", 5)]
    [InlineData("\x1b[=5mprivate", "private", 7)]
    [InlineData("\x1b[38:2::255:0:0mcolon", "\x1b[38:2::255:0:0mcolon", 5)]
    [InlineData("\x1b]0;title\x07text", "text", 4)]
    [InlineData("\x1b]8;;https://example.com\x1b\\link\x1b]8;;\x1b\\", "link", 4)]
    [InlineData("\u001b7saved\u001b8", "saved", 5)]
    [InlineData("\x1b(Bcharset", "charset", 7)]
    [InlineData("\x1b[12", "", 0)]
    [InlineData("\x1b[12\x07x", "\u2407x", 2)]
    [InlineData("end\x1b", "end\u241b", 4)]
    public void Normalize_EscapeSequences_KeepOnlySgr(string text, string expected, int width)
    {
        var line = Assert.Single(ConsoleText.Normalize(text, 0));

        Assert.Equal(expected, line);
        Assert.Equal(width, ConsoleText.MeasureWidth(line));
    }

    // Each line is written separately and the console driver resets colors after every write, so the
    // effective SGR state at a line break is restored at the start of the next line, as one sequence.
    [Theory]
    [InlineData("\x1b[31mfirst\nsecond\x1b[0m", new[] { "\x1b[31mfirst", "\x1b[31msecond\x1b[0m" })]
    [InlineData("\x1b[1m\x1b[31mA\nB\nC", new[] { "\x1b[1m\x1b[31mA", "\x1b[1;31mB", "\x1b[1;31mC" })]
    [InlineData("\x1b[31m\x1b[32mA\nB", new[] { "\x1b[31m\x1b[32mA", "\x1b[32mB" })]
    [InlineData("\x1b[1mA\x1b[0m\nB", new[] { "\x1b[1mA\x1b[0m", "B" })]
    [InlineData("\x1b[1mA\x1b[m\nB", new[] { "\x1b[1mA\x1b[m", "B" })]
    [InlineData("\x1b[1mA\x1b[0;32m\nB", new[] { "\x1b[1mA\x1b[0;32m", "\x1b[32mB" })]
    [InlineData("\x1b[1;2;31mA\x1b[22;39m\nB", new[] { "\x1b[1;2;31mA\x1b[22;39m", "B" })]
    [InlineData("\x1b[38;5;208;48;2;1;2;3mA\nB", new[] { "\x1b[38;5;208;48;2;1;2;3mA", "\x1b[38;5;208;48;2;1;2;3mB" })]
    [InlineData("\x1b[38:2::255:0:0;4:3mA\nB", new[] { "\x1b[38:2::255:0:0;4:3mA", "\x1b[4:3;38:2::255:0:0mB" })]
    [InlineData("\x1b[3;7;9mA\x1b[23mB\nC", new[] { "\x1b[3;7;9mA\x1b[23mB", "\x1b[7;9mC" })]
    public void Normalize_SgrStateSpanningLineBreaks_IsRestored(string text, string[] expected)
    {
        Assert.Equal(expected, ConsoleText.Normalize(text, 0));
    }

    [Fact]
    public void Normalize_RepeatedStyleChanges_DoNotGrowContinuationLines()
    {
        // Replaying the whole SGR history made each line longer than the last (quadratic output).
        var text = string.Concat(Enumerable.Repeat("\x1b[31mx\x1b[39m\n", 1024)) + "\x1b[1;31m" + string.Concat(Enumerable.Repeat("\x1b[32m\x1b[33my\n", 1024));

        var lines = ConsoleText.Normalize(text, 0);

        Assert.True(lines.Sum(line => line.Length) < text.Length * 2);
        Assert.Equal("\x1b[1;33m", lines[^1]);
    }

    [Fact]
    public void Normalize_ManyTabs_ExpandToEightColumnsEach()
    {
        // Each tab used to remeasure the whole line so far, which was quadratic.
        var line = Assert.Single(ConsoleText.Normalize(new string('\t', 10_000), 0));

        Assert.Equal(80_000, line.Length);
    }

    [Theory]
    [InlineData("\x1b[31mab\x1b[0m\tx", "\x1b[31mab\x1b[0m      x")]
    [InlineData("あい\tう\tx", "あい    う      x")]
    [InlineData("👨‍👩‍👧‍👦\tx", "👨‍👩‍👧‍👦      x")]
    public void Normalize_TabsAfterStylesAndWideText_CountDisplayColumns(string text, string expected)
    {
        Assert.Equal([expected], ConsoleText.Normalize(text, 0));
    }

    [Theory]
    [InlineData("a\0b", "a\u2400b")]
    [InlineData("\a\b\f\v", "\u2407\u2408\u240c\u240b")]
    [InlineData("del\x7f", "del\u2421")]
    [InlineData("c1\u009b", "c1\ufffd")]
    public void Normalize_OtherControlCharacters_BecomeVisibleSymbols(string text, string expected)
    {
        var line = Assert.Single(ConsoleText.Normalize(text, 0));

        Assert.Equal(expected, line);
        Assert.Equal(expected.Length, ConsoleText.MeasureWidth(line));
    }

    [Theory]
    [InlineData("日本語", 6)]
    [InlineData("👨‍👩‍👧‍👦", 2)]
    [InlineData("🇯🇵", 2)]
    [InlineData("é", 1)]
    public void Normalize_PrintableText_IsUnchanged(string text, int width)
    {
        var line = Assert.Single(ConsoleText.Normalize(text, 0));

        Assert.Same(text, line);
        Assert.Equal(width, ConsoleText.MeasureWidth(line));
    }
}
