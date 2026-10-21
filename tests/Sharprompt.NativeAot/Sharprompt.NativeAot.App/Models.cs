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

// The trim analyzer reports IL2026 wherever MinLength, MaxLength or Length is applied, whether or not Sharprompt
// is used, because their constructors are [RequiresUnreferencedCode]. It is suppressed here so that the check is
// that the binder Sharprompt generates for these attributes adds no warning of its own.
#pragma warning disable IL2026
[PromptBindable]
public class SignupModel
{
    [Display(Name = "Name", Order = 1)]
    [Required]
    [StringLength(20)]
    [MinLength(2)]
    public string? Name { get; set; }

    [Display(Name = "Age", Order = 2)]
    [Range(0, 150)]
    public int Age { get; set; }

    [Display(Name = "Secret", Order = 3)]
    [DataType(DataType.Password)]
    [Required]
    [MaxLength(16)]
    public string? Secret { get; set; }

    [Display(Name = "Priority", Order = 4)]
    public LocalPriority Priority { get; set; }

    [Display(Name = "External priority", Order = 5)]
    public ExternalPriority ExternalPriority { get; set; }

    [Display(Name = "Languages", Order = 6)]
    [InlineItems("C#", "F#", "VB")]
    public string[]? Languages { get; set; }

    [Display(Name = "Tags", Order = 7)]
    [Length(1, 8)]
    public List<string>? Tags { get; set; }

    [Display(Name = "Subscribe", Order = 8)]
    public bool Subscribe { get; set; }
}
#pragma warning restore IL2026

public static class LocalStrings
{
    public static string Morning => System.Globalization.CultureInfo.CurrentUICulture.Name == "ja-JP" ? "朝" : "Morning";

    public static string Evening => System.Globalization.CultureInfo.CurrentUICulture.Name == "ja-JP" ? "夜" : "Evening";
}

// Display names come from a resource type, so the generator leaves this enum to the reflection fallback.
// ExternalTimeOfDay is the same enum in another assembly.
public enum LocalTimeOfDay
{
    [Display(Name = nameof(LocalStrings.Morning), ResourceType = typeof(LocalStrings))]
    Morning,

    [Display(Name = nameof(LocalStrings.Evening), ResourceType = typeof(LocalStrings))]
    Evening
}
