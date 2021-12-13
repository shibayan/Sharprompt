using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sharprompt.Example.Models
{
    public class MyFormModel
    {
        [Display(Name = "What's your name?", Order = 1)]
        [Required]
        public string Name { get; set; }

        [Display(Name = "Type new password", Order = 2)]
        [DataType(DataType.Password)]
        [Required]
        [MinLength(8)]
        public string Password { get; set; }

        [Display(Name = "Select enum value", Order = 3)]
        public MyEnum MyEnum { get; set; }

        [Display(Name = "Select enum values", Order = 4)]
        public IEnumerable<MyEnum> MyEnums { get; set; }

        [Display(Name = "Please add item(s)", Order = 5)]
        public IEnumerable<string> Lists { get; set; }

        [Display(Name = "Are you ready?", Order = 10)]
        public bool Ready { get; set; }
    }
}
