using System.ComponentModel.DataAnnotations;

namespace Sharprompt.Example.Models
{
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
