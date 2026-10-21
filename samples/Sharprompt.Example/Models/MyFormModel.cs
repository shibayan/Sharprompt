using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sharprompt.Example.Models;

[PromptBindable]
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
    // MinLength reads Count through reflection only for values that are neither strings nor ICollection, so the
    // trim analyzer flags every use of it. A string property never takes that path.
#pragma warning disable IL2026
    [MinLength(8)]
#pragma warning restore IL2026
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
