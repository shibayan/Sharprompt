using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;

using Sharprompt;
using Sharprompt.Automation;

// This program runs without a terminal. Unexpected answers or exhausted input fail the process.
var previousFactory = Prompt.ConsoleDriverFactory;
var previousCancel = Prompt.ThrowExceptionOnCancel;

try
{
    Prompt.ThrowExceptionOnCancel = true;

    // A prompt owns and disposes its driver. Queue a fresh one for each prompt.
    Prompt.ConsoleDriverFactory = () => new ScriptedConsoleDriver().Type("Ada").Enter();
    var name = Prompt.Input<string>("Name", validators: [Validators.Required()]);
    if (name != "Ada")
    {
        throw new InvalidOperationException("Unexpected input result.");
    }

    // No item is highlighted initially: the first Down selects Seattle, the second Tokyo.
    Prompt.ConsoleDriverFactory = () => new ScriptedConsoleDriver().Key(ConsoleKey.DownArrow).Key(ConsoleKey.DownArrow).Enter();
    var city = Prompt.Select("City", new[] { "Seattle", "Tokyo" });
    if (city != "Tokyo")
    {
        throw new InvalidOperationException("Unexpected selection.");
    }

    Prompt.ConsoleDriverFactory = () => new ScriptedConsoleDriver().Key(ConsoleKey.DownArrow).Key(ConsoleKey.Spacebar, ' ').Enter();
    if (!Prompt.MultiSelect("Cities", new[] { "Tokyo", "London" }).SequenceEqual(new[] { "Tokyo" }))
    {
        throw new InvalidOperationException("Unexpected multiple selection.");
    }

    Prompt.ConsoleDriverFactory = () => new ScriptedConsoleDriver().Enter();
    if (!Prompt.Confirm("Continue?", defaultValue: true))
    {
        throw new InvalidOperationException("Unexpected confirmation.");
    }

    Prompt.ConsoleDriverFactory = () => new ScriptedConsoleDriver().Type("example-password").Enter();
    if (Prompt.Password("Password", validators: [Validators.MinLength(8)]) != "example-password")
    {
        throw new InvalidOperationException("Unexpected password.");
    }

    Prompt.ConsoleDriverFactory = () => new ScriptedConsoleDriver().Type("42").Enter().Enter();
    if (!Prompt.List<int>("Numbers").SequenceEqual(new[] { 42 }))
    {
        throw new InvalidOperationException("Unexpected list.");
    }

    // The first Enter fails validation; the same prompt then accepts the scripted text.
    Prompt.ConsoleDriverFactory = () => new ScriptedConsoleDriver().Enter().Type("Ada").Enter();
    if (Prompt.Input<string>("Required name", validators: [Validators.Required()]) != "Ada")
    {
        throw new InvalidOperationException("Validation retry failed.");
    }

    Prompt.ConsoleDriverFactory = () => new ScriptedConsoleDriver().Type("Ada").Enter();
    if (Prompt.Bind<Profile>().Name != "Ada")
    {
        throw new InvalidOperationException("Source-generated binding failed.");
    }

    // Exercise cancellation after the prompt has started, with no timer or sleep.
    using var cancellation = new CancellationTokenSource();
    Prompt.ConsoleDriverFactory = () => new ScriptedConsoleDriver { OnRead = cancellation.Cancel };
    try
    {
        Prompt.Input<string>(options => options.Message = "Canceled input", cancellation.Token);
        throw new InvalidOperationException("Expected token cancellation.");
    }
    catch (OperationCanceledException ex) when (ex.CancellationToken == cancellation.Token)
    {
    }
}
finally
{
    Prompt.ConsoleDriverFactory = previousFactory;
    Prompt.ThrowExceptionOnCancel = previousCancel;
}

Console.WriteLine("All automation examples passed.");

[PromptBindable]
public class Profile
{
    [Required]
    public string Name { get; set; } = "";
}
