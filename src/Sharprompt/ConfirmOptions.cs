namespace Sharprompt;

/// <summary>
/// Settings for a yes-or-no prompt.
/// </summary>
public class ConfirmOptions : PromptOptions
{
    /// <summary>
    /// The answer accepted by Enter. Defaults to null, which requires an explicit answer.
    /// </summary>
    public bool? DefaultValue { get; set; }
}
