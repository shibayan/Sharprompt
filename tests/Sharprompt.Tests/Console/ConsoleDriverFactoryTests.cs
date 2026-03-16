using System;

using Sharprompt.Drivers;

using Xunit;

namespace Sharprompt.Tests;

public class ConsoleDriverFactoryTests
{
    [Fact]
    public void ConsoleDriverFactory_DefaultFactory_IsNotNull()
    {
        Assert.NotNull(Prompt.ConsoleDriverFactory);
    }

    [Fact]
    public void ConsoleDriverFactory_SetCustomFactory_ReturnsCustomFactory()
    {
        var originalFactory = Prompt.ConsoleDriverFactory;

        try
        {
            var customDriver = new StubConsoleDriver();
            Prompt.ConsoleDriverFactory = () => customDriver;

            var driver = Prompt.ConsoleDriverFactory();

            Assert.Same(customDriver, driver);
        }
        finally
        {
            Prompt.ConsoleDriverFactory = originalFactory;
        }
    }

    [Fact]
    public void ConsoleDriverFactory_SetNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => Prompt.ConsoleDriverFactory = null!);
    }

    [Fact]
    public void ConsoleDriverFactory_AfterReset_ReturnsPreviousFactory()
    {
        var originalFactory = Prompt.ConsoleDriverFactory;

        try
        {
            Prompt.ConsoleDriverFactory = () => new StubConsoleDriver();

            Prompt.ConsoleDriverFactory = originalFactory;

            Assert.Same(originalFactory, Prompt.ConsoleDriverFactory);
        }
        finally
        {
            Prompt.ConsoleDriverFactory = originalFactory;
        }
    }

    [Fact]
    public void IConsoleDriver_CustomImplementation_CanBeAssigned()
    {
        var originalFactory = Prompt.ConsoleDriverFactory;

        try
        {
            IConsoleDriver driver = new StubConsoleDriver();
            Prompt.ConsoleDriverFactory = () => driver;

            var result = Prompt.ConsoleDriverFactory();

            Assert.IsAssignableFrom<IConsoleDriver>(result);
            Assert.Same(driver, result);
        }
        finally
        {
            Prompt.ConsoleDriverFactory = originalFactory;
        }
    }

    private sealed class StubConsoleDriver : IConsoleDriver
    {
        public void Dispose() { }
        public void Beep() { }
        public void Reset() { }
        public void ClearLine(int top) { }
        public ConsoleKeyInfo ReadKey() => new('\0', ConsoleKey.Enter, false, false, false);
        public void Write(string value, ConsoleColor color) { }
        public void WriteLine() { }
        public void SetCursorPosition(int left, int top) { }
        public bool KeyAvailable => false;
        public bool CursorVisible { set { } }
        public int CursorLeft => 0;
        public int CursorTop => 0;
        public int BufferWidth => 80;
        public int BufferHeight => 24;
        public int WindowWidth => 80;
        public int WindowHeight => 24;
        public Action CancellationCallback { get; set; } = () => { };
    }
}
