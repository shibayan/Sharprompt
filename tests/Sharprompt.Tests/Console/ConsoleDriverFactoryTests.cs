using System;
using System.Threading;

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

    public static TheoryData<string, Action<CancellationToken>> PromptEntryPoints => new()
    {
        { nameof(Prompt.Input), token => Prompt.Input<string>(new InputOptions<string> { Message = "message" }, token) },
        { nameof(Prompt.Password), token => Prompt.Password(new PasswordOptions { Message = "message" }, token) },
        { nameof(Prompt.Confirm), token => Prompt.Confirm(new ConfirmOptions { Message = "message" }, token) },
        { nameof(Prompt.Select), token => Prompt.Select(new SelectOptions<string> { Message = "message", Items = ["a"] }, token) },
        { nameof(Prompt.MultiSelect), token => Prompt.MultiSelect(new MultiSelectOptions<string> { Message = "message", Items = ["a"] }, token) },
        { nameof(Prompt.List), token => Prompt.List(new ListOptions<string> { Message = "message" }, token) },
        { nameof(Prompt.Bind), token => Prompt.Bind(new PreCanceledBindModel(), token) }
    };

    [Theory]
    [MemberData(nameof(PromptEntryPoints))]
    public void PromptEntryPoint_PreCanceledToken_DoesNotInvokeConsoleDriverFactory(string name, Action<CancellationToken> prompt)
    {
        var originalFactory = Prompt.ConsoleDriverFactory;
        var factoryInvoked = false;

        // Creates the console driver directly, so Bind itself has to observe the pre-canceled token.
        ModelBinderRegistry.Register<PreCanceledBindModel>((_, _) => Prompt.ConsoleDriverFactory().Dispose());

        try
        {
            Prompt.ConsoleDriverFactory = () =>
            {
                factoryInvoked = true;

                return new StubConsoleDriver();
            };

            using var cts = new CancellationTokenSource();
            cts.Cancel();

            var exception = Assert.Throws<OperationCanceledException>(() => prompt(cts.Token));

            Assert.Equal(cts.Token, exception.CancellationToken);
            Assert.False(factoryInvoked, $"Prompt.{name} invoked ConsoleDriverFactory.");
        }
        finally
        {
            Prompt.ConsoleDriverFactory = originalFactory;
        }
    }

    public class PreCanceledBindModel;

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
