using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Sharprompt.NativeAot.External;

// Mirrors LocalTimeOfDay in the app: display names come from a resource type and follow the UI culture.
public static class ExternalStrings
{
    public static string Morning => CultureInfo.CurrentUICulture.Name == "ja-JP" ? "朝" : "Morning";

    public static string Evening => CultureInfo.CurrentUICulture.Name == "ja-JP" ? "夜" : "Evening";
}

public enum ExternalTimeOfDay
{
    [Display(Name = nameof(ExternalStrings.Morning), ResourceType = typeof(ExternalStrings))]
    Morning,

    [Display(Name = nameof(ExternalStrings.Evening), ResourceType = typeof(ExternalStrings))]
    Evening
}
