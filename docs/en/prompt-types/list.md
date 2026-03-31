# List

The `List` prompt lets the user add multiple items interactively.

## Basic Usage

```csharp
var value = Prompt.List<string>("Please add item(s)");
Console.WriteLine($"You picked {string.Join(", ", value)}");
```

## Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `message` | `string` | The message to display to the user |
| `minimum` | `int` | Minimum number of items required (default: `1`) |
| `maximum` | `int` | Maximum number of items allowed (default: unlimited) |
| `validators` | `IList<Func<object?, ValidationResult?>>?` | List of validators for each item |

## Options Class

```csharp
var value = Prompt.List(new ListOptions<string>
{
    Message = "Please add item(s)",
    Minimum = 1,
    Maximum = 5,
    Validators = { Validators.Required() }
});
```

## Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `DefaultValues` | `IEnumerable<T>` | `[]` | Items pre-populated in the list |
| `Minimum` | `int` | `1` | Minimum number of items required |
| `Maximum` | `int` | `int.MaxValue` | Maximum number of items allowed |
| `Validators` | `IList<Func<object?, ValidationResult?>>` | `[]` | Validators for each item |

## Fluent API

```csharp
using Sharprompt.Fluent;

var value = Prompt.List<string>(o => o.WithMessage("Please add item(s)")
                                      .WithMinimum(1)
                                      .WithMaximum(5));
```
