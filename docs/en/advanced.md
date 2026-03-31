# Advanced Features

## Enum Type Support

When using an enum type with `Select` or `MultiSelect`, items are automatically generated from the enum values. You can use the `[Display]` attribute to customize the display names:

```csharp
using System.ComponentModel.DataAnnotations;

public enum MyEnum
{
    [Display(Name = "First value")]
    First,
    [Display(Name = "Second value")]
    Second,
    [Display(Name = "Third value")]
    Third
}

var value = Prompt.Select<MyEnum>("Select enum value");
Console.WriteLine($"You selected {value}");
```

For `MultiSelect`:

```csharp
var values = Prompt.MultiSelect<MyEnum>("Select enum values");
Console.WriteLine($"You picked {string.Join(", ", values)}");
```

## Unicode Support

Sharprompt supports multi-byte characters and emoji. For best results, set the output encoding to UTF-8:

```csharp
Console.OutputEncoding = Encoding.UTF8;

var name = Prompt.Input<string>("What's your name?");
Console.WriteLine($"Hello, {name}!");
```

## Fluent Interface

All prompt types support a fluent interface via the `Sharprompt.Fluent` namespace:

```csharp
using Sharprompt.Fluent;

var city = Prompt.Select<string>(o => o.WithMessage("Select your city")
                                       .WithItems(new[] { "Seattle", "London", "Tokyo" })
                                       .WithDefaultValue("Seattle"));
```

The fluent API provides `With*` methods for setting properties and `Add*` methods for adding to collections (like validators).

## Text Selector

For `Select` and `MultiSelect`, you can provide a custom function to control how items are displayed:

```csharp
var user = Prompt.Select("Select user", users, textSelector: u => u.Name);
```

## Pagination

Both `Select` and `MultiSelect` support pagination via the `pageSize` parameter:

```csharp
var city = Prompt.Select("Select your city",
    new[] { "Seattle", "London", "Tokyo", "New York", "Singapore", "Shanghai" },
    pageSize: 3);
```

## Looping Selection

By default, selection lists loop when reaching the beginning or end. This can be disabled via the options class:

```csharp
var city = Prompt.Select(new SelectOptions<string>
{
    Message = "Select your city",
    Items = new[] { "Seattle", "London", "Tokyo" },
    LoopingSelection = false
});
```
