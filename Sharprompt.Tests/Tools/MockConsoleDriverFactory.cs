using System;
using Sharprompt.Drivers;

namespace Sharprompt.Tests
{
    public class MockConsoleDriverFactory : IConsoleDriverFactory
    {
        private readonly Func<IConsoleDriver> _consoleDriverFn;

        /// <summary>
        /// Initializes a new instance of the <see cref="MockConsoleDriverFactory" /> class.
        /// </summary>
        public MockConsoleDriverFactory(Func<IConsoleDriver> consoleDriverFn)
        {
            _consoleDriverFn = consoleDriverFn;
        }

        public IConsoleDriver Create() => _consoleDriverFn();
    }
}
