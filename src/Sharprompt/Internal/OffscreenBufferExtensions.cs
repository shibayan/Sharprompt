namespace Sharprompt.Internal;

internal static class OffscreenBufferExtensions
{
    public static void WritePrompt(this OffscreenBuffer offscreenBuffer, string message)
    {
        offscreenBuffer.Write(offscreenBuffer.Configuration.Symbols.Prompt, offscreenBuffer.Configuration.ColorSchema.PromptSymbol);
        offscreenBuffer.Write($" {message}: ");
    }

    public static void WriteDone(this OffscreenBuffer offscreenBuffer, string message)
    {
        offscreenBuffer.Write(offscreenBuffer.Configuration.Symbols.Done, offscreenBuffer.Configuration.ColorSchema.DoneSymbol);
        offscreenBuffer.Write($" {message}: ");
    }

    public static void WriteError(this OffscreenBuffer offscreenBuffer, string message) => offscreenBuffer.Write($"{offscreenBuffer.Configuration.Symbols.Error} {message}", offscreenBuffer.Configuration.ColorSchema.Error);

    public static void WriteAnswer(this OffscreenBuffer offscreenBuffer, string message) => offscreenBuffer.Write(message, offscreenBuffer.Configuration.ColorSchema.Answer);

    public static void WriteSelect(this OffscreenBuffer offscreenBuffer, string message) => offscreenBuffer.Write(message, offscreenBuffer.Configuration.ColorSchema.Select);

    public static void WriteHint(this OffscreenBuffer offscreenBuffer, string message) => offscreenBuffer.Write(message, offscreenBuffer.Configuration.ColorSchema.Hint);

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
