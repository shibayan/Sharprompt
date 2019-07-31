using System;

using Sharprompt;

namespace Example
{
    class Program
    {
        static void Main(string[] args)
        {
            var name = Prompt.Input<string>("What's your name", validators: new[] { Validators.Required(), Validators.MinLength(3) });
            var age = Prompt.Input<int>("How old are you");

            var city = Prompt.Select("Choose a city", new[] { "Seattle", "London", "Tokyo" });

            var password = Prompt.Password("Type new password", new[] { Validators.Required(), Validators.MinLength(8) });

            var ready = Prompt.Confirm("Are you ready");

            Console.WriteLine($"Name: {name}, Age: {age}, City: {city}\nPassword: {password}, Ready: {ready}");
        }
    }
}
