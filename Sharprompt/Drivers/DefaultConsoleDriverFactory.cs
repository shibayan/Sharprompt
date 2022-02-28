using System;
using System.Runtime.InteropServices;

using Sharprompt.Internal;

namespace Sharprompt.Drivers
{
    internal sealed class DefaultConsoleDriverFactory : IConsoleDriverFactory
    {
        private bool isInitialized = false;

        private void InitializeDefaultConsoleDriver()
        {
            if (isInitialized) return;

            if (Console.IsInputRedirected || Console.IsOutputRedirected)
            {
                throw new InvalidOperationException("Sharprompt requires an interactive environment.");
            }

            Console.TreatControlCAsInput = true;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var hConsole = NativeMethods.GetStdHandle(NativeMethods.STD_OUTPUT_HANDLE);

                if (!NativeMethods.GetConsoleMode(hConsole, out var mode))
                {
                    return;
                }

                NativeMethods.SetConsoleMode(hConsole, mode | NativeMethods.ENABLE_VIRTUAL_TERMINAL_PROCESSING);
            }

            isInitialized = true;
        }

        public IConsoleDriver Create()
        {
            InitializeDefaultConsoleDriver();
            return new DefaultConsoleDriver();
        }
    }
}
