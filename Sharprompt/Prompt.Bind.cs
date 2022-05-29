namespace Sharprompt;

public static partial class Prompt
{
    public static T Bind<T>() where T : new() => PromptRealisation.Bind<T>();

    public static T Bind<T>(T model) => PromptRealisation.Bind<T>(model);
}
