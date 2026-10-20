using System.ComponentModel.DataAnnotations;

namespace Sharprompt.NativeAot.ExternalEnums;

// Mirrors LocalPriority in the app, so both enum metadata paths can be checked against the same expectations.
public enum ExternalPriority
{
    Low = 0,

    // Alias of Low. "Default" sorts before "Low" ordinally, so it represents the value.
    [Display(Name = "Default priority")]
    Default = 0,

    [Display(Name = "Medium priority", Order = 2)]
    Medium = 1,

    [Display(Name = "High priority", Order = 1)]
    High = 2
}
