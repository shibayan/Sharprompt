using Sharprompt.Internal;

using Xunit;

namespace Sharprompt.Tests
{
    public class TextInputBufferTests
    {
        [Theory]
        [InlineData("a", 1)]
        [InlineData("abc", 3)]
        [InlineData("あ", 1)]
        [InlineData("あいう", 3)]
        [InlineData("🍣", 2)]
        [InlineData("🍣🍖🥂", 6)]
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
    }
}
