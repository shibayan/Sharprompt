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
