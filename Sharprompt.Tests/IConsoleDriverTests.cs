using System;

using Sharprompt.Drivers;

using Xunit;

namespace Sharprompt.Tests;

public class IConsoleDriverTests
{
    [Fact]
    public void Beep_CanBeCalled()
    {
        var driver = new RecordingConsoleDriver();

        ((IConsoleDriver)driver).Beep();

        Assert.True(driver.BeepCalled);
    }

    [Fact]
    public void Reset_CanBeCalled()
    {
        var driver = new RecordingConsoleDriver();

        ((IConsoleDriver)driver).Reset();

        Assert.True(driver.ResetCalled);
    }

    [Fact]
    public void ClearLine_PassesCorrectArgument()
    {
        var driver = new RecordingConsoleDriver();

        ((IConsoleDriver)driver).ClearLine(5);

        Assert.Equal(5, driver.ClearLineTop);
    }

    [Fact]
    public void ReadKey_ReturnsConfiguredKeyInfo()
    {
        var expected = new ConsoleKeyInfo('A', ConsoleKey.A, false, false, false);
        var driver = new RecordingConsoleDriver { ReadKeyResult = expected };

        var result = ((IConsoleDriver)driver).ReadKey();

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Write_PassesCorrectArguments()
    {
        var driver = new RecordingConsoleDriver();

        ((IConsoleDriver)driver).Write("hello", ConsoleColor.Cyan);

        Assert.Equal("hello", driver.LastWriteValue);
        Assert.Equal(ConsoleColor.Cyan, driver.LastWriteColor);
    }

    [Fact]
    public void WriteLine_CanBeCalled()
    {
        var driver = new RecordingConsoleDriver();

        ((IConsoleDriver)driver).WriteLine();

        Assert.True(driver.WriteLineCalled);
    }

    [Fact]
    public void SetCursorPosition_PassesCorrectCoordinates()
    {
        var driver = new RecordingConsoleDriver();

        ((IConsoleDriver)driver).SetCursorPosition(10, 20);

        Assert.Equal(10, driver.SetCursorLeft);
        Assert.Equal(20, driver.SetCursorTop);
    }

    [Fact]
    public void KeyAvailable_ReturnsConfiguredValue()
    {
        var driver = new RecordingConsoleDriver { KeyAvailableValue = true };

        var result = ((IConsoleDriver)driver).KeyAvailable;

        Assert.True(result);
    }

    [Fact]
    public void CursorVisible_CanBeSet()
    {
        var driver = new RecordingConsoleDriver();

        ((IConsoleDriver)driver).CursorVisible = false;

        Assert.False(driver.CursorVisibleValue);
    }

    [Fact]
    public void CursorLeft_ReturnsConfiguredValue()
    {
        var driver = new RecordingConsoleDriver { CursorLeftValue = 42 };

        var result = ((IConsoleDriver)driver).CursorLeft;

        Assert.Equal(42, result);
    }

    [Fact]
    public void CursorTop_ReturnsConfiguredValue()
    {
        var driver = new RecordingConsoleDriver { CursorTopValue = 7 };

        var result = ((IConsoleDriver)driver).CursorTop;

        Assert.Equal(7, result);
    }

    [Fact]
    public void BufferWidth_ReturnsConfiguredValue()
    {
        var driver = new RecordingConsoleDriver { BufferWidthValue = 120 };

        var result = ((IConsoleDriver)driver).BufferWidth;

        Assert.Equal(120, result);
    }

    [Fact]
    public void BufferHeight_ReturnsConfiguredValue()
    {
        var driver = new RecordingConsoleDriver { BufferHeightValue = 50 };

        var result = ((IConsoleDriver)driver).BufferHeight;

        Assert.Equal(50, result);
    }

    [Fact]
    public void WindowWidth_ReturnsConfiguredValue()
    {
        var driver = new RecordingConsoleDriver { WindowWidthValue = 200 };

        var result = ((IConsoleDriver)driver).WindowWidth;

        Assert.Equal(200, result);
    }

    [Fact]
    public void WindowHeight_ReturnsConfiguredValue()
    {
        var driver = new RecordingConsoleDriver { WindowHeightValue = 60 };

        var result = ((IConsoleDriver)driver).WindowHeight;

        Assert.Equal(60, result);
    }

    [Fact]
    public void CancellationCallback_CanBeSetAndInvoked()
    {
        var driver = new RecordingConsoleDriver();
        var invoked = false;
        Action callback = () => invoked = true;

        ((IConsoleDriver)driver).CancellationCallback = callback;
        ((IConsoleDriver)driver).CancellationCallback.Invoke();

        Assert.True(invoked);
    }

    [Fact]
    public void Dispose_CanBeCalled()
    {
        var driver = new RecordingConsoleDriver();

        ((IDisposable)driver).Dispose();

        Assert.True(driver.DisposeCalled);
    }

    private sealed class RecordingConsoleDriver : IConsoleDriver
    {
        public bool BeepCalled { get; private set; }
        public bool ResetCalled { get; private set; }
        public int ClearLineTop { get; private set; } = -1;
        public ConsoleKeyInfo ReadKeyResult { get; set; } = new('\0', ConsoleKey.Enter, false, false, false);
        public string? LastWriteValue { get; private set; }
        public ConsoleColor LastWriteColor { get; private set; }
        public bool WriteLineCalled { get; private set; }
        public int SetCursorLeft { get; private set; } = -1;
        public int SetCursorTop { get; private set; } = -1;
        public bool KeyAvailableValue { get; set; }
        public bool? CursorVisibleValue { get; private set; }
        public int CursorLeftValue { get; set; }
        public int CursorTopValue { get; set; }
        public int BufferWidthValue { get; set; } = 80;
        public int BufferHeightValue { get; set; } = 24;
        public int WindowWidthValue { get; set; } = 80;
        public int WindowHeightValue { get; set; } = 24;
        public bool DisposeCalled { get; private set; }

        public void Dispose() => DisposeCalled = true;
        public void Beep() => BeepCalled = true;
        public void Reset() => ResetCalled = true;
        public void ClearLine(int top) => ClearLineTop = top;
        public ConsoleKeyInfo ReadKey() => ReadKeyResult;
        public void Write(string value, ConsoleColor color) { LastWriteValue = value; LastWriteColor = color; }
        public void WriteLine() => WriteLineCalled = true;
        public void SetCursorPosition(int left, int top) { SetCursorLeft = left; SetCursorTop = top; }
        public bool KeyAvailable => KeyAvailableValue;
        public bool CursorVisible { set => CursorVisibleValue = value; }
        public int CursorLeft => CursorLeftValue;
        public int CursorTop => CursorTopValue;
        public int BufferWidth => BufferWidthValue;
        public int BufferHeight => BufferHeightValue;
        public int WindowWidth => WindowWidthValue;
        public int WindowHeight => WindowHeightValue;
        public Action CancellationCallback { get; set; } = () => { };
    }
}
