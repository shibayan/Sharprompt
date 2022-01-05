namespace Sharprompt.Drivers
{
    public static class ConsoleDriverFactory
    {
        public static IConsoleDriverFactory Instance { get; set; } = new DefaultConsoleDriverFactory();
    }
}
