using System;
using System.Text;

using Sharprompt.Example.Models;

namespace Sharprompt.Example
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            RunInputSample();

            RunSelectSample();

            RunPasswordSample();

            RunConfirmSample();

            RunMultiSelectSample();

            RunSelectEnumSample();

            RunMultiSelectEnumSample();

            RunAutoFormsSample();
        }

        private static void RunInputSample()
        {
            var name = Prompt.Input<string>("What's your name?", validators: new[] { Validators.Required(), Validators.MinLength(3) });
            Console.WriteLine($"Hello, {name}!");
        }

        private static void RunSelectSample()
        {
            var city = Prompt.Select("Select your city", new[] { "Seattle", "London", "Tokyo", "New York", "Singapore", "Shanghai" }, pageSize: 3);
            Console.WriteLine($"Hello, {city}!");
        }

        private static void RunPasswordSample()
        {
            var secret = Prompt.Password("Type new password", new[] { Validators.Required(), Validators.MinLength(8) });
            Console.WriteLine("Password OK");
        }

        private static void RunConfirmSample()
        {
            var answer = Prompt.Confirm("Are you ready?");
            Console.WriteLine($"Your answer is {answer}");
        }

        private static void RunMultiSelectSample()
        {
            var options = Prompt.MultiSelect("Which cities would you like to visit?", new[] { "Seattle", "London", "Tokyo", "New York", "Singapore", "Shanghai" }, pageSize: 3);
            Console.WriteLine($"You picked {string.Join(", ", options)}");
        }

        private static void RunSelectEnumSample()
        {
            var value = Prompt.Select<MyEnum>("Select enum value", defaultValue: MyEnum.Bar);
            Console.WriteLine($"You selected {value}");
        }

        private static void RunMultiSelectEnumSample()
        {
            var value = Prompt.MultiSelect<MyEnum>("Select enum value");
            Console.WriteLine($"You picked {string.Join(", ", value)}");
        }

        private static void RunAutoFormsSample()
        {
            var model = Prompt.AutoForms<MyFormModel>();
        }
    }
}
