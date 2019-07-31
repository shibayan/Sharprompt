using System;

using Sharprompt;

namespace Example
{
    class Program
    {
        static void Main(string[] args)
        {
            var name = Prompt.Input<string>("Name", "kazuakix");
            var age = Prompt.Input<int>("Age", 50);

            var prefecture = Prompt.Select("Choose a prefecture", new[] { "Osaka", "Hyogo", "Wakayama", "Nara", "Kyoto" }, "Wakayama");

            var ready = Prompt.Confirm("Ossan", true);

            var password = Prompt.Password("Enter password");

            Console.WriteLine($"Name: {name}, Age: {age}, Prefecture: {prefecture}\nOssan: {ready}, Password: {password}");
        }
    }
}
