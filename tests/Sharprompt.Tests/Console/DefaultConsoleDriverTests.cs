using System;

using Sharprompt.Drivers;

using Xunit;

namespace Sharprompt.Tests;

public class DefaultConsoleDriverTests
{
    [Fact]
    public void Constructor_RedirectedConsole_ThrowsInvalidOperationException()
    {
        Assert.SkipUnless(Console.IsInputRedirected || Console.IsOutputRedirected, "Requires redirected console I/O.");

        // The exception must surface directly (not wrapped in TypeInitializationException)
        // and must not poison the type for later attempts.
        Assert.Throws<InvalidOperationException>(() => new DefaultConsoleDriver());
        Assert.Throws<InvalidOperationException>(() => new DefaultConsoleDriver());
    }
}
