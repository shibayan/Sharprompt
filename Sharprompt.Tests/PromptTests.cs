using System.Threading;
using System.Threading.Tasks;
using Sharprompt.Drivers;
using Xunit;

namespace Sharprompt.Tests
{
    public class PromptTests
    {
        private MockConsoleDriver ConsoleDriver { get; }

        public PromptTests()
        {
            ConsoleDriver = new MockConsoleDriver();
            ConsoleDriverFactory.Instance = new MockConsoleDriverFactory(() => ConsoleDriver);
        }

        [Fact]
        public async Task InputString()
        {
            var keyWait = new AutoResetEvent(false);
            ConsoleDriver.AwaitingKeyPress += (sender, args) => keyWait.Set();

            var task = Task.Run(() => Prompt.Input<string>("Get"));

            keyWait.WaitOne();
            Assert.Equal("? Get:", ConsoleDriver.GetOutputBuffer());

            ConsoleDriver.InputBuffer.WriteLine("PASS");
            var result = await task;

            Assert.Equal("PASS", result);
            Assert.Equal("V Get: PASS", ConsoleDriver.GetOutputBuffer());
        }

        [Fact]
        public async Task InputInteger()
        {
            var keyWait = new AutoResetEvent(false);
            ConsoleDriver.AwaitingKeyPress += (sender, args) => keyWait.Set();

            var task = Task.Run(() => Prompt.Input<int>("Get"));

            keyWait.WaitOne();
            Assert.Equal("? Get:", ConsoleDriver.GetOutputBuffer());

            ConsoleDriver.InputBuffer.WriteLine("0");
            var result = await task;

            Assert.Equal(0, result);
            Assert.Equal("V Get: 0", ConsoleDriver.GetOutputBuffer());
        }

        [Fact]
        public void InputInteger_ValidationFailed()
        {
            var keyWait = new AutoResetEvent(false);

            ConsoleDriver.AwaitingKeyPress += (sender, args) => keyWait.Set();

            Task.Run(() => Prompt.Input<int>("Get"));

            keyWait.WaitOne();
            Assert.Equal("? Get:", ConsoleDriver.GetOutputBuffer());

            ConsoleDriver.InputBuffer.WriteLine("PASS");

            keyWait.WaitOne();
            Assert.Equal("? Get: PASS\n>> PASS is not a valid value for Int32. (Parameter 'value')", ConsoleDriver.GetOutputBuffer());
        }
    }
}
