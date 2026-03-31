# Select

The `Select` prompt lets the user choose a single item from a list.

## Basic Usage

```csharp
var city = Prompt.Select("Select your city", new[] { "Seattle", "London", "Tokyo" });
Console.WriteLine($"Hello, {city}!");
```

## Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `message` | `string` | The message to display to the user |
| `items` | `IEnumerable<T>?` | Items to select from (auto-generated for enum types) |
| `pageSize` | `int` | Number of items to display per page (default: unlimited) |
| `defaultValue` | `object?` | Default selected value |
| `textSelector` | `Func<T, string>?` | Function to convert items to display text |

## Options Class

```csharp
var city = Prompt.Select(new SelectOptions<string>
{
    Message = "Select your city",
    Items = new[] { "Seattle", "London", "Tokyo", "New York", "Singapore", "Shanghai" },
    PageSize = 3,
    DefaultValue = "Seattle"
});
```

## Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Items` | `IEnumerable<T>` | — | Items to select from |
| `DefaultValue` | `object?` | `null` | Default selected value |
| `PageSize` | `int` | `int.MaxValue` | Number of items per page |
| `TextSelector` | `Func<T, string>` | `x => x.ToString()!` | Display text selector |
| `LoopingSelection` | `bool` | `true` | Whether to loop at the beginning/end of the list |

## With Pagination

```csharp
var city = Prompt.Select("Select your city",
    new[] { "Seattle", "London", "Tokyo", "New York", "Singapore", "Shanghai" },
    pageSize: 3);
Console.WriteLine($"Hello, {city}!");
```

## With Enum Type

When using an enum type, items are automatically generated:

```csharp
var value = Prompt.Select<MyEnum>("Select enum value");
Console.WriteLine($"You selected {value}");
```

See [Enum Support](/advanced#enum-type-support) for more details.

## Fluent API

```csharp
using Sharprompt.Fluent;

var city = Prompt.Select<string>(o => o.WithMessage("Select your city")
                                       .WithItems(new[] { "Seattle", "London", "Tokyo" })
                                       .WithDefaultValue("Seattle"));
```
