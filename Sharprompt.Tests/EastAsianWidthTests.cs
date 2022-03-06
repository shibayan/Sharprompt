using Sharprompt.Internal;

using Xunit;

namespace Sharprompt.Tests;

public class EastAsianWidthTests
{
    [Theory]
    [InlineData("a", 1)]
    [InlineData("abc", 3)]
    [InlineData("あ", 2)]
    [InlineData("あいう", 6)]
    [InlineData("𩸽", 2)]
    [InlineData("𩸽𠈻𠮷", 6)]
    [InlineData("🍣", 2)]
    [InlineData("🍣🍖🥂", 6)]
    [InlineData("aあ𩸽🍣", 7)]
    public void GetWidth(string value, int width)
    {
        Assert.Equal(width, value.GetWidth());
    }
}
