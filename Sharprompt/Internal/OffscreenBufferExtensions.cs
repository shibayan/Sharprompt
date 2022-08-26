namespace Sharprompt.Internal;

internal static class OffscreenBufferExtensions
{
    public static void WritePrompt(this OffscreenBuffer offscreenBuffer, string message)
    {
        offscreenBuffer.Write(Prompt.Symbols.Prompt, Prompt.ColorSchema.PromptSymbol);
        offscreenBuffer.Write($" {message}: ");
    }

    public static void WriteDone(this OffscreenBuffer offscreenBuffer, string message)
    {
        offscreenBuffer.Write(Prompt.Symbols.Done, Prompt.ColorSchema.DoneSymbol);
        offscreenBuffer.Write($" {message}: ");
    }

    public static void WriteError(this OffscreenBuffer offscreenBuffer, string message) => offscreenBuffer.Write($"{Prompt.Symbols.Error} {message}", Prompt.ColorSchema.Error);

    public static void WriteAnswer(this OffscreenBuffer offscreenBuffer, string message) => offscreenBuffer.Write(message, Prompt.ColorSchema.Answer);

    public static void WriteSelect(this OffscreenBuffer offscreenBuffer, string message) => offscreenBuffer.Write(message, Prompt.ColorSchema.Select);

    public static void WriteHint(this OffscreenBuffer offscreenBuffer, string message) => offscreenBuffer.Write(message, Prompt.ColorSchema.Hint);

    public static void WriteInput(this OffscreenBuffer offscreenBuffer, TextInputBuffer textInputBuffer)
    {
        offscreenBuffer.Write(textInputBuffer.ToBackwardString());

        offscreenBuffer.PushCursor();

        offscreenBuffer.Write(textInputBuffer.ToForwardString());
    }
}
