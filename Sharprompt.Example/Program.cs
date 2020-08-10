using System;
using System.ComponentModel.DataAnnotations;
using System.Text;

using Sharprompt.Validations;

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

            RunAutoFormsSample();
        }

        private static void RunInputSample()
        {
            var name = Prompt.Input<string>("What's your name?", validators: new[] { Validators.Required() });
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
            var value = Prompt.Select<MyEnum>("Select enum value");
            Console.WriteLine($"You selected {value}");
        }

        private static void RunAutoFormsSample()
        {
            var model = Prompt.AutoForms<FormModel>();
        }
    }

    public class FormModel
    {
        [Display(Description = "What's your name?", Order = 1)]
        public string Name { get; set; }

        [Display(Description = "Select enum value", Order = 2)]
        public MyEnum MyEnum { get; set; }

        [Display(Description = "Type new password", Order = 3)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Description = "Are you ready?", Order = 4)]
        public bool Ready { get; set; }
    }

    public enum MyEnum
    {
        [Display(Name = "Foo value")]
        Foo,

        [Display(Name = "Bar value")]
        Bar,

        [Display(Name = "Baz value")]
        Baz
    }
}
