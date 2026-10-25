namespace Sharprompt.Internal;

/// <summary>
/// Writes prompt components using the configured symbols, colors, and editable-text cursor position.
/// </summary>
internal static class OffscreenBufferExtensions
{
    /// <summary>
    /// Writes the active-prompt symbol and the prompt message followed by its input separator.
    /// </summary>
    /// <param name="offscreenBuffer">The pending output buffer to write or render.</param>
    /// <param name="message">The component text to display.</param>
    public static void WritePrompt(this OffscreenBuffer offscreenBuffer, string message)
    {
        offscreenBuffer.Write(offscreenBuffer.Configuration.Symbols.Prompt, offscreenBuffer.Configuration.ColorSchema.PromptSymbol);
        offscreenBuffer.Write($" {message}: ");
    }

    /// <summary>
    /// Writes the completed-prompt symbol and message followed by its answer separator.
    /// </summary>
    /// <param name="offscreenBuffer">The pending output buffer to write or render.</param>
    /// <param name="message">The component text to display.</param>
    public static void WriteDone(this OffscreenBuffer offscreenBuffer, string message)
    {
        offscreenBuffer.Write(offscreenBuffer.Configuration.Symbols.Done, offscreenBuffer.Configuration.ColorSchema.DoneSymbol);
        offscreenBuffer.Write($" {message}: ");
    }

    /// <summary>
    /// Writes a validation error with the configured error symbol and color.
    /// </summary>
    /// <param name="offscreenBuffer">The pending output buffer to write or render.</param>
    /// <param name="message">The component text to display.</param>
    public static void WriteError(this OffscreenBuffer offscreenBuffer, string message) => offscreenBuffer.Write($"{offscreenBuffer.Configuration.Symbols.Error} {message}", offscreenBuffer.Configuration.ColorSchema.Error);

    /// <summary>
    /// Writes accepted answer text with the configured answer color.
    /// </summary>
    /// <param name="offscreenBuffer">The pending output buffer to write or render.</param>
    /// <param name="message">The component text to display.</param>
    public static void WriteAnswer(this OffscreenBuffer offscreenBuffer, string message) => offscreenBuffer.Write(message, offscreenBuffer.Configuration.ColorSchema.Answer);

    /// <summary>
    /// Writes selection text with the configured highlight color.
    /// </summary>
    /// <param name="offscreenBuffer">The pending output buffer to write or render.</param>
    /// <param name="message">The component text to display.</param>
    public static void WriteSelect(this OffscreenBuffer offscreenBuffer, string message) => offscreenBuffer.Write(message, offscreenBuffer.Configuration.ColorSchema.Select);

    /// <summary>
    /// Writes supporting text with the configured hint color.
    /// </summary>
    /// <param name="offscreenBuffer">The pending output buffer to write or render.</param>
    /// <param name="message">The component text to display.</param>
    public static void WriteHint(this OffscreenBuffer offscreenBuffer, string message) => offscreenBuffer.Write(message, offscreenBuffer.Configuration.ColorSchema.Hint);

    /// <summary>
    /// Writes both sides of the edit cursor, recording its position and showing ESC as a visible symbol.
    /// </summary>
    /// <param name="offscreenBuffer">The pending output buffer to write or render.</param>
    /// <param name="textInputBuffer">The editable text and its current cursor position.</param>
    public static void WriteInput(this OffscreenBuffer offscreenBuffer, TextInputBuffer textInputBuffer)
    {
        // The text before and after the cursor is written separately, which would split an escape sequence
        // the cursor is inside. Text being edited shows ESC as ␛ instead, so every character stays visible.
        offscreenBuffer.Write(ShowEscape(textInputBuffer.ToBackwardString()));

        offscreenBuffer.PushCursor();

        offscreenBuffer.Write(ShowEscape(textInputBuffer.ToForwardString()));
    }

    private static string ShowEscape(string text) => text.Replace('\x1b', (char)0x241b);
}
