using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sharprompt.Example.Models;

public class MyFormModel
{
    [Display(Name = "Id")]
    [BindIgnore]
    public int Id { get; set; }

    public string ReadOnly { get; } = null!;

    [Display(Name = "What's your name?", Prompt = "Required", Order = 1)]
    [Required]
    public string Name { get; set; } = null!;

    [Display(Name = "Type new password", Order = 2)]
    [DataType(DataType.Password)]
    [Required]
    [MinLength(8)]
    public string Password { get; set; } = null!;

    [Display(Name = "Select enum value", Order = 3)]
    public MyEnum? MyEnum { get; set; }

    [Display(Name = "Select enum values", Order = 4)]
    public IEnumerable<MyEnum> MyEnums { get; set; } = null!;

    [Display(Name = "Please add item(s)", Order = 5)]
    public IEnumerable<string> Lists { get; set; } = null!;

    [Display(Name = "Are you ready?", Order = 10)]
    public bool? Ready { get; set; }
}
