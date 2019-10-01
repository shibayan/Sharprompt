using System;

using Sharprompt;

namespace Example
{
    class Program
    {
        static void Main(string[] args)
        {
            var name = Prompt.Input<string>("What's your name?", validators: new[] { Validators.Required() });
            Console.WriteLine($"Hello, {name}!");

            var city = Prompt.Select("Select your city", new[] { "Seattle", "London", "Tokyo", "New York", "Singapore", "Shanghai" }, pageSize: 3);
            Console.WriteLine($"Hello, {city}!");

            var secret = Prompt.Password("Type new password", new[] { Validators.Required(), Validators.MinLength(8) });
            Console.WriteLine("Password OK");

            var answer = Prompt.Confirm("Are you ready?");
            Console.WriteLine($"Your answer is {answer}");

            var options = Prompt.MultiSelect("Which cities would you like to visit?", new[] { "Seattle", "London", "Tokyo", "New York", "Singapore", "Shanghai" }, pageSize: 3);
            Console.WriteLine($"You picked {string.Join(", ", options)}");
        }
    }
}
