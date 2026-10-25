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
| `LoopingSelection` | `bool` | `true` | Whether Up/Down wraps within the current page (`true`) or moves between pages (`false`); page navigation still wraps between the first and last pages |

## Selection Rules

- `Items` is read once, when the prompt starts. Equal items (compared with `EqualityComparer<T>.Default`) are listed once, at the position of the first one.
- `DefaultValues` that are not in `Items` are ignored.
- The result contains the selected items in the order of `Items`.
- `Minimum` and `Maximum` limit the number of selected items:

| Operation | Result |
|-----------|--------|
| <kbd>Space</kbd> on an unchecked item at `Maximum` | The item is not checked, and an error is shown. |
| <kbd>Ctrl</kbd>+<kbd>A</kbd> or <kbd>Ctrl</kbd>+<kbd>I</kbd> that would exceed `Maximum` | Nothing changes, and an error is shown. |
| More `DefaultValues` than `Maximum` | They are all checked. <kbd>Enter</kbd> is rejected until enough are unchecked. |
| <kbd>Enter</kbd> below `Minimum` or above `Maximum` | Rejected with an error. |

<kbd>Ctrl</kbd>+<kbd>A</kbd> and <kbd>Ctrl</kbd>+<kbd>I</kbd> only affect the items that match the current filter. Checked items hidden by the filter stay checked.

Before the prompt is shown, `ArgumentOutOfRangeException` is thrown when `Minimum` is negative, when `Maximum` is less than `Minimum`, or when `Minimum` is greater than the number of items, because the prompt could not be answered. For an empty `Items`, set `Minimum` to `0`.

## With Enum Type

```csharp
var values = Prompt.MultiSelect<MyEnum>("Select enum values", defaultValues: new[] { MyEnum.Bar });
Console.WriteLine($"You picked {string.Join(", ", values)}");
```

Items are ordered the same way as for `Select`. See [Item Order](/advanced#item-order).

## Fluent API

```csharp
using Sharprompt.Fluent;

var cities = Prompt.MultiSelect<string>(o => o.WithMessage("Which cities would you like to visit?")
                                               .WithItems(new[] { "Seattle", "London", "Tokyo" })
                                               .WithPageSize(3));
```
