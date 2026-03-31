# MultiSelect

The `MultiSelect` prompt lets the user choose multiple items from a list using checkboxes.

## Basic Usage

```csharp
var cities = Prompt.MultiSelect("Which cities would you like to visit?",
    new[] { "Seattle", "London", "Tokyo", "New York", "Singapore", "Shanghai" },
    pageSize: 3);
Console.WriteLine($"You picked {string.Join(", ", cities)}");
```

## Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `message` | `string` | The message to display to the user |
| `items` | `IEnumerable<T>?` | Items to select from (auto-generated for enum types) |
| `pageSize` | `int` | Number of items to display per page (default: unlimited) |
| `minimum` | `int` | Minimum number of items that must be selected (default: `1`) |
| `maximum` | `int` | Maximum number of items that can be selected (default: unlimited) |
| `defaultValues` | `IEnumerable<T>?` | Items selected by default |
| `textSelector` | `Func<T, string>?` | Function to convert items to display text |

## Options Class

```csharp
var cities = Prompt.MultiSelect(new MultiSelectOptions<string>
{
    Message = "Which cities would you like to visit?",
    Items = new[] { "Seattle", "London", "Tokyo", "New York", "Singapore", "Shanghai" },
    PageSize = 3,
    Minimum = 1,
    Maximum = 3,
    DefaultValues = new[] { "Tokyo" }
});
```

## Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Items` | `IEnumerable<T>` | — | Items to select from |
| `DefaultValues` | `IEnumerable<T>` | `[]` | Items selected by default |
| `PageSize` | `int` | `int.MaxValue` | Number of items per page |
| `Minimum` | `int` | `1` | Minimum number of selections required |
| `Maximum` | `int` | `int.MaxValue` | Maximum number of selections allowed |
| `TextSelector` | `Func<T, string>` | `x => x.ToString()!` | Display text selector |
| `LoopingSelection` | `bool` | `true` | Whether to loop at the beginning/end of the list |

## With Enum Type

```csharp
var values = Prompt.MultiSelect<MyEnum>("Select enum values", defaultValues: new[] { MyEnum.Bar });
Console.WriteLine($"You picked {string.Join(", ", values)}");
```

## Fluent API

```csharp
using Sharprompt.Fluent;

var cities = Prompt.MultiSelect<string>(o => o.WithMessage("Which cities would you like to visit?")
                                               .WithItems(new[] { "Seattle", "London", "Tokyo" })
                                               .WithPageSize(3));
```
