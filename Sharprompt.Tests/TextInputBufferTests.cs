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

            while (!textInputBuffer.IsStart)
            {
                textInputBuffer.MoveBackward();
            }

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

            while (!textInputBuffer.IsStart)
            {
                textInputBuffer.MoveBackward();
            }

            textInputBuffer.MoveForward();

            Assert.Equal(backward, textInputBuffer.ToBackwardString());
            Assert.Equal(forward, textInputBuffer.ToForwardString());
        }
    }
}
