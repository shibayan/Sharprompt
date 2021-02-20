using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sharprompt.Example.Models
{
    public class MyFormModel
    {
        [Display(Prompt = "What's your name?", Order = 1)]
        [Required]
        public string Name { get; set; }

        [Display(Prompt = "Type new password", Order = 2)]
        [DataType(DataType.Password)]
        [Required]
        [MinLength(8)]
        public string Password { get; set; }

        [Display(Prompt = "Select enum value", Order = 3)]
        public MyEnum? MyEnum { get; set; }

        [Display(Prompt = "Select enum values", Order = 4)]
        public IEnumerable<MyEnum> MyEnums { get; set; }

        [Display(Prompt = "Are you ready?", Order = 5)]
        public bool Ready { get; set; }
    }
}
