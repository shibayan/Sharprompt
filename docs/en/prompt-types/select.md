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
| `LoopingSelection` | `bool` | `true` | Whether Up/Down wraps within the current page (`true`) or moves between pages (`false`); page navigation still wraps between the first and last pages |

## With Pagination

```csharp
var city = Prompt.Select("Select your city",
    new[] { "Seattle", "London", "Tokyo", "New York", "Singapore", "Shanghai" },
    pageSize: 3);
Console.WriteLine($"Hello, {city}!");
```

## Item Rules

- `Items` is read once, when the prompt starts. Equal items (compared with `EqualityComparer<T>.Default`) are listed once, at the position of the first one.
- `DefaultValue` selects the matching item when the prompt starts. If it is not in `Items`, it does not select anything. A value of a compatible type, such as an `int` for a `long` prompt, is converted; an incompatible one throws `ArgumentException`.
- <kbd>Enter</kbd> with nothing selected shows an error. When only one item is listed, with or without a filter, or when the filter exactly matches an item's text, that item is selected.
- An empty `Items` throws `ArgumentException` before the prompt is shown, because nothing could be selected.

## With Enum Type

When using an enum type, items are automatically generated:

```csharp
var value = Prompt.Select<MyEnum>("Select enum value");
Console.WriteLine($"You selected {value}");
```

Items are ordered by value, with `[Display(Order)]` members first. See [Item Order](/advanced#item-order) for the exact rules, and [Enum Support](/advanced#enum-type-support) for more details.

## Fluent API

```csharp
using Sharprompt.Fluent;

var city = Prompt.Select<string>(o => o.WithMessage("Select your city")
                                       .WithItems(new[] { "Seattle", "London", "Tokyo" })
                                       .WithDefaultValue("Seattle"));
```
