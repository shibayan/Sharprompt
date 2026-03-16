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
        offscreenBuffer.Write(textInputBuffer.ToBackwardString());

        offscreenBuffer.PushCursor();

        offscreenBuffer.Write(textInputBuffer.ToForwardString());
    }
}
