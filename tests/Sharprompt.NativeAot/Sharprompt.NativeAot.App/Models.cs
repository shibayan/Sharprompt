using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Sharprompt.NativeAot.External;

namespace Sharprompt.NativeAot;

// Declared in this project, so the packaged source generator registers its metadata.
// ExternalPriority is the same enum declared in another assembly, which goes through the reflection fallback.
public enum LocalPriority
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

[PromptBindable]
public class SignupModel
{
    [Display(Name = "Name", Order = 1)]
    [Required]
    [StringLength(20)]
    public string? Name { get; set; }

    [Display(Name = "Age", Order = 2)]
    [Range(0, 150)]
    public int Age { get; set; }

    [Display(Name = "Secret", Order = 3)]
    [DataType(DataType.Password)]
    [Required]
    public string? Secret { get; set; }

    [Display(Name = "Priority", Order = 4)]
    public LocalPriority Priority { get; set; }

    [Display(Name = "External priority", Order = 5)]
    public ExternalPriority ExternalPriority { get; set; }

    [Display(Name = "Languages", Order = 6)]
    [InlineItems("C#", "F#", "VB")]
    public string[]? Languages { get; set; }

    [Display(Name = "Tags", Order = 7)]
    public List<string>? Tags { get; set; }

    [Display(Name = "Subscribe", Order = 8)]
    public bool Subscribe { get; set; }
}

public static class LocalStrings
{
    public static string Morning => System.Globalization.CultureInfo.CurrentUICulture.Name == "ja-JP" ? "朝" : "Morning";

    public static string Evening => System.Globalization.CultureInfo.CurrentUICulture.Name == "ja-JP" ? "夜" : "Evening";
}

// Display names come from a resource type. ExternalTimeOfDay is the same enum in another assembly.
public enum LocalTimeOfDay
{
    [Display(Name = nameof(LocalStrings.Morning), ResourceType = typeof(LocalStrings))]
    Morning,

    [Display(Name = nameof(LocalStrings.Evening), ResourceType = typeof(LocalStrings))]
    Evening
}
