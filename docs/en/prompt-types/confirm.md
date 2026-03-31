# Confirm

The `Confirm` prompt asks the user a yes/no question.

## Basic Usage

```csharp
var answer = Prompt.Confirm("Are you ready?");
Console.WriteLine($"Your answer is {answer}");
```

## With Default Value

```csharp
var answer = Prompt.Confirm("Are you ready?", defaultValue: true);
Console.WriteLine($"Your answer is {answer}");
```

## Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `message` | `string` | The message to display to the user |
| `defaultValue` | `bool?` | Default value if the user presses Enter without input |

## Options Class

```csharp
var answer = Prompt.Confirm(new ConfirmOptions
{
    Message = "Are you ready?",
    DefaultValue = true
});
```

## Fluent API

```csharp
using Sharprompt.Fluent;

var answer = Prompt.Confirm(o => o.WithMessage("Are you ready?")
                                  .WithDefaultValue(true));
```
