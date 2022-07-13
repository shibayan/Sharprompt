using Sharprompt.Internal;

using Xunit;

namespace Sharprompt.Tests;

public class TextInputBufferTests
{
    [Theory]
    [InlineData("a", 1)]
    [InlineData("abc", 3)]
    [InlineData("あ", 1)]
    [InlineData("あいう", 3)]
    [InlineData("𩸽", 2)]
    [InlineData("𩸽𠈻𠮷", 6)]
    [InlineData("🍣", 2)]
    [InlineData("🍣🍖🥂", 6)]
    [InlineData("aあ𩸽🍣", 6)]
    public void Insert(string value, int length)
    {
        var textInputBuffer = new TextInputBuffer();

        foreach (var c in value)
        {
            textInputBuffer.Insert(c);
        }

        Assert.Equal(length, textInputBuffer.Length);
        Assert.Equal(value, textInputBuffer.ToString());
    }

    [Theory]
    [InlineData("abc", "ab")]
    [InlineData("あいう", "あい")]
    [InlineData("𩸽𠈻𠮷", "𩸽𠈻")]
    [InlineData("🍣🍖🥂", "🍣🍖")]
    [InlineData("aあ𩸽🍣", "aあ𩸽")]
    public void Backspace(string value, string substring)
    {
        var textInputBuffer = new TextInputBuffer();

        foreach (var c in value)
        {
            textInputBuffer.Insert(c);
        }

        textInputBuffer.Backspace();

        Assert.Equal(substring, textInputBuffer.ToString());
        Assert.Equal(substring, textInputBuffer.ToBackwardString());
        Assert.Equal(string.Empty, textInputBuffer.ToForwardString());
    }

    [Theory]
    [InlineData("abc", "bc")]
    [InlineData("あいう", "いう")]
    [InlineData("𩸽𠈻𠮷", "𠈻𠮷")]
    [InlineData("🍣🍖🥂", "🍖🥂")]
    [InlineData("aあ𩸽🍣", "あ𩸽🍣")]
    public void Delete(string value, string substring)
    {
        var textInputBuffer = new TextInputBuffer();

        foreach (var c in value)
        {
            textInputBuffer.Insert(c);
        }

        textInputBuffer.MoveToStart();

        textInputBuffer.Delete();

        Assert.Equal(substring, textInputBuffer.ToString());
        Assert.Equal(string.Empty, textInputBuffer.ToBackwardString());
        Assert.Equal(substring, textInputBuffer.ToForwardString());
    }

    [Theory]
    [InlineData("abc", "ab", "c")]
    [InlineData("あいう", "あい", "う")]
    [InlineData("𩸽𠈻𠮷", "𩸽𠈻", "𠮷")]
    [InlineData("🍣🍖🥂", "🍣🍖", "🥂")]
    [InlineData("aあ𩸽🍣", "aあ𩸽", "🍣")]
    public void MoveBackward(string value, string backward, string forward)
    {
        var textInputBuffer = new TextInputBuffer();

        foreach (var c in value)
        {
            textInputBuffer.Insert(c);
        }

        textInputBuffer.MoveBackward();

        Assert.Equal(backward, textInputBuffer.ToBackwardString());
        Assert.Equal(forward, textInputBuffer.ToForwardString());
    }

    [Theory]
    [InlineData("abc", "a", "bc")]
    [InlineData("あいう", "あ", "いう")]
    [InlineData("𩸽𠈻𠮷", "𩸽", "𠈻𠮷")]
    [InlineData("🍣🍖🥂", "🍣", "🍖🥂")]
    [InlineData("aあ𩸽🍣", "a", "あ𩸽🍣")]
    public void MoveForward(string value, string backward, string forward)
    {
        var textInputBuffer = new TextInputBuffer();

        foreach (var c in value)
        {
            textInputBuffer.Insert(c);
        }

        textInputBuffer.MoveToStart();

        textInputBuffer.MoveForward();

        Assert.Equal(backward, textInputBuffer.ToBackwardString());
        Assert.Equal(forward, textInputBuffer.ToForwardString());
    }

    [Theory]
    [InlineData("_abc", "", "abc")]
    [InlineData("a_bc", "", "abc")]
    [InlineData("abc_", "", "abc")]
    [InlineData("_あいう", "", "あいう")]
    [InlineData("あ_いう", "", "あいう")]
    [InlineData("あいう_", "", "あいう")]
    [InlineData("_𩸽𠈻𠮷", "", "𩸽𠈻𠮷")]
    [InlineData("𩸽_𠈻𠮷", "", "𩸽𠈻𠮷")]
    [InlineData("𩸽𠈻𠮷_", "", "𩸽𠈻𠮷")]
    [InlineData("_🍣🍖🥂", "", "🍣🍖🥂")]
    [InlineData("🍣_🍖🥂", "", "🍣🍖🥂")]
    [InlineData("🍣🍖🥂_", "", "🍣🍖🥂")]
    [InlineData("_aあ𩸽🍣", "", "aあ𩸽🍣")]
    [InlineData("a_あ𩸽🍣", "", "aあ𩸽🍣")]
    [InlineData("aあ_𩸽🍣", "", "aあ𩸽🍣")]
    [InlineData("aあ𩸽_🍣", "", "aあ𩸽🍣")]
    [InlineData("aあ𩸽🍣_", "", "aあ𩸽🍣")]
    [InlineData("_ abc def ", "", " abc def ")]
    [InlineData(" _abc def ", "", " abc def ")]
    [InlineData(" a_bc def ", " ", "abc def ")]
    [InlineData(" abc_ def ", " ", "abc def ")]
    [InlineData(" abc _def ", " ", "abc def ")]
    [InlineData(" abc d_ef ", " abc ", "def ")]
    [InlineData(" abc def_ ", " abc ", "def ")]
    [InlineData(" abc def _", " abc ", "def ")]
    public void MoveToPreviousWord(string value, string backward, string forward)
    {
        var textInputBuffer = new TextInputBuffer();
        var cursor = value.IndexOf('_');
        foreach (var c in value[(cursor + 1)..])
        {
            textInputBuffer.Insert(c);
        }
        textInputBuffer.MoveToStart();
        foreach (var c in value[..cursor])
        {
            textInputBuffer.Insert(c);
        }

        textInputBuffer.MoveToPreviousWord();

        Assert.Equal(backward, textInputBuffer.ToBackwardString());
        Assert.Equal(forward, textInputBuffer.ToForwardString());
    }

    [Theory]
    [InlineData("_abc", "abc", "")]
    [InlineData("a_bc", "abc", "")]
    [InlineData("abc_", "abc", "")]
    [InlineData("_あいう", "あいう", "")]
    [InlineData("あ_いう", "あいう", "")]
    [InlineData("あいう_", "あいう", "")]
    [InlineData("_𩸽𠈻𠮷", "𩸽𠈻𠮷", "")]
    [InlineData("𩸽_𠈻𠮷", "𩸽𠈻𠮷", "")]
    [InlineData("𩸽𠈻𠮷_", "𩸽𠈻𠮷", "")]
    [InlineData("_🍣🍖🥂", "🍣🍖🥂", "")]
    [InlineData("🍣_🍖🥂", "🍣🍖🥂", "")]
    [InlineData("🍣🍖🥂_", "🍣🍖🥂", "")]
    [InlineData("_aあ𩸽🍣", "aあ𩸽🍣", "")]
    [InlineData("a_あ𩸽🍣", "aあ𩸽🍣", "")]
    [InlineData("aあ_𩸽🍣", "aあ𩸽🍣", "")]
    [InlineData("aあ𩸽_🍣", "aあ𩸽🍣", "")]
    [InlineData("aあ𩸽🍣_", "aあ𩸽🍣", "")]
    [InlineData("_ abc def ", " ", "abc def ")]
    [InlineData(" _abc def ", " abc ", "def ")]
    [InlineData(" a_bc def ", " abc ", "def ")]
    [InlineData(" abc_ def ", " abc ", "def ")]
    [InlineData(" abc _def ", " abc def ", "")]
    [InlineData(" abc d_ef ", " abc def ", "")]
    [InlineData(" abc def_ ", " abc def ", "")]
    [InlineData(" abc def _", " abc def ", "")]
    public void MoveToNextWord(string value, string backward, string forward)
    {
        var textInputBuffer = new TextInputBuffer();
        var cursor = value.IndexOf('_');
        foreach (var c in value[(cursor + 1)..])
        {
            textInputBuffer.Insert(c);
        }
        textInputBuffer.MoveToStart();
        foreach (var c in value[..cursor])
        {
            textInputBuffer.Insert(c);
        }

        textInputBuffer.MoveToNextWord();

        Assert.Equal(backward, textInputBuffer.ToBackwardString());
        Assert.Equal(forward, textInputBuffer.ToForwardString());
    }

    [Theory]
    [InlineData("_abc", "", "abc")]
    [InlineData("a_bc", "", "bc")]
    [InlineData("abc_", "", "")]
    [InlineData("_あいう", "", "あいう")]
    [InlineData("あ_いう", "", "いう")]
    [InlineData("あいう_", "", "")]
    [InlineData("_𩸽𠈻𠮷", "", "𩸽𠈻𠮷")]
    [InlineData("𩸽_𠈻𠮷", "", "𠈻𠮷")]
    [InlineData("𩸽𠈻𠮷_", "", "")]
    [InlineData("_🍣🍖🥂", "", "🍣🍖🥂")]
    [InlineData("🍣_🍖🥂", "", "🍖🥂")]
    [InlineData("🍣🍖🥂_", "", "")]
    [InlineData("_aあ𩸽🍣", "", "aあ𩸽🍣")]
    [InlineData("a_あ𩸽🍣", "", "あ𩸽🍣")]
    [InlineData("aあ_𩸽🍣", "", "𩸽🍣")]
    [InlineData("aあ𩸽_🍣", "", "🍣")]
    [InlineData("aあ𩸽🍣_", "", "")]
    [InlineData("_ abc def ", "", " abc def ")]
    [InlineData(" _abc def ", "", "abc def ")]
    [InlineData(" a_bc def ", " ", "bc def ")]
    [InlineData(" abc_ def ", " ", " def ")]
    [InlineData(" abc _def ", " ", "def ")]
    [InlineData(" abc d_ef ", " abc ", "ef ")]
    [InlineData(" abc def_ ", " abc ", " ")]
    [InlineData(" abc def _", " abc ", "")]
    public void BackspaceWord(string value, string backward, string forward)
    {
        var textInputBuffer = new TextInputBuffer();
        var cursor = value.IndexOf('_');
        foreach (var c in value[(cursor + 1)..])
        {
            textInputBuffer.Insert(c);
        }
        textInputBuffer.MoveToStart();
        foreach (var c in value[..cursor])
        {
            textInputBuffer.Insert(c);
        }

        textInputBuffer.BackspaceWord();

        Assert.Equal(backward, textInputBuffer.ToBackwardString());
        Assert.Equal(forward, textInputBuffer.ToForwardString());
    }

    [Theory]
    [InlineData("_abc", "", "")]
    [InlineData("a_bc", "a", "")]
    [InlineData("abc_", "abc", "")]
    [InlineData("_あいう", "", "")]
    [InlineData("あ_いう", "あ", "")]
    [InlineData("あいう_", "あいう", "")]
    [InlineData("_𩸽𠈻𠮷", "", "")]
    [InlineData("𩸽_𠈻𠮷", "𩸽", "")]
    [InlineData("𩸽𠈻𠮷_", "𩸽𠈻𠮷", "")]
    [InlineData("_🍣🍖🥂", "", "")]
    [InlineData("🍣_🍖🥂", "🍣", "")]
    [InlineData("🍣🍖🥂_", "🍣🍖🥂", "")]
    [InlineData("_aあ𩸽🍣", "", "")]
    [InlineData("a_あ𩸽🍣", "a", "")]
    [InlineData("aあ_𩸽🍣", "aあ", "")]
    [InlineData("aあ𩸽_🍣", "aあ𩸽", "")]
    [InlineData("aあ𩸽🍣_", "aあ𩸽🍣", "")]
    [InlineData("_ abc def ", "", "abc def ")]
    [InlineData(" _abc def ", " ", "def ")]
    [InlineData(" a_bc def ", " a", "def ")]
    [InlineData(" abc_ def ", " abc", "def ")]
    [InlineData(" abc _def ", " abc ", "")]
    [InlineData(" abc d_ef ", " abc d", "")]
    [InlineData(" abc def_ ", " abc def", "")]
    [InlineData(" abc def _", " abc def ", "")]
    public void DeleteWord(string value, string backward, string forward)
    {
        var textInputBuffer = new TextInputBuffer();
        var cursor = value.IndexOf('_');
        foreach (var c in value[(cursor + 1)..])
        {
            textInputBuffer.Insert(c);
        }
        textInputBuffer.MoveToStart();
        foreach (var c in value[..cursor])
        {
            textInputBuffer.Insert(c);
        }

        textInputBuffer.DeleteWord();

        Assert.Equal(backward, textInputBuffer.ToBackwardString());
        Assert.Equal(forward, textInputBuffer.ToForwardString());
    }
}
