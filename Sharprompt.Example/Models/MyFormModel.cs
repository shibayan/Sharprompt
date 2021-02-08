using System.ComponentModel.DataAnnotations;

namespace Sharprompt.Example.Models
{
    public class MyFormModel
    {
        [Display(Description = "What's your name?", Order = 1)]
        [Required]
        public string Name { get; set; }

        [Display(Description = "Select enum value", Order = 2)]
        public MyEnum MyEnum { get; set; }

        [Display(Description = "Type new password", Order = 3)]
        [DataType(DataType.Password)]
        [Required]
        [MinLength(8)]
        public string Password { get; set; }

        [Display(Description = "Are you ready?", Order = 4)]
        public bool Ready { get; set; }
    }
}
