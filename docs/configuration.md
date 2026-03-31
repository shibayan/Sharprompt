# Configuration

Sharprompt allows you to customize the appearance and behavior of prompts globally.

## Symbols

You can customize the symbols used by prompts:

```csharp
Prompt.Symbols.Prompt = new Symbol("🤔", "?");
Prompt.Symbols.Done = new Symbol("😎", "V");
Prompt.Symbols.Error = new Symbol("😱", ">>");

var name = Prompt.Input<string>("What's your name?");
```

The `Symbol` class takes two arguments: a Unicode value and a fallback value for terminals that don't support Unicode.

### Available Symbols

| Symbol | Default (Unicode) | Default (Fallback) | Description |
|--------|-------------------|-------------------|-------------|
| `Prompt` | `?` | `?` | Displayed before the prompt message |
| `Done` | `✔` | `V` | Displayed when the prompt is completed |
| `Error` | `»` | `>>` | Displayed when validation fails |
| `Selector` | `›` | `>` | Points to the currently highlighted item |
| `Selected` | `◉` | `(*)` | Marks a selected item in MultiSelect |
| `NotSelect` | `◯` | `( )` | Marks an unselected item in MultiSelect |

## Color Schema

Customize the colors used by prompts:

```csharp
Prompt.ColorSchema.Answer = ConsoleColor.DarkRed;
Prompt.ColorSchema.Select = ConsoleColor.DarkCyan;

var name = Prompt.Input<string>("What's your name?");
```

### Available Colors

| Property | Default | Description |
|----------|---------|-------------|
| `DoneSymbol` | `Green` | Color of the done symbol |
| `PromptSymbol` | `Green` | Color of the prompt symbol |
| `Answer` | `Cyan` | Color of the user's answer |
| `Select` | `Green` | Color of the selected item |
| `Error` | `Red` | Color of error messages |
| `Hint` | `DarkGray` | Color of hints and placeholders |
| `DisabledOption` | `DarkCyan` | Color of disabled options |

## Cancellation Support

By default, pressing `Ctrl+C` returns the default value. You can change this behavior to throw an exception:

```csharp
Prompt.ThrowExceptionOnCancel = true;

try
{
    var name = Prompt.Input<string>("What's your name?");
    Console.WriteLine($"Hello, {name}!");
}
catch (PromptCanceledException ex)
{
    Console.WriteLine("Prompt canceled");
}
```

## Culture

Set the culture for localized messages:

```csharp
Prompt.Culture = new CultureInfo("ja");
```

## Console Driver

You can provide a custom console driver factory:

```csharp
Prompt.ConsoleDriverFactory = () => new MyCustomConsoleDriver();
```
