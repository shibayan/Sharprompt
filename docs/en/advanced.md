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

### Item Order

Enum items are listed in the same order whether the metadata comes from the Sharprompt source generator or from the reflection fallback (used when the generator did not run, for example for enums in another assembly):

1. Members are ordered by their underlying value, not by declaration order. Negative values come first for signed enums. Composite values of `[Flags]` enums are ordinary members and are sorted with the rest.
2. Aliases (members that share a value) appear once. The alias whose name sorts first in ordinal order represents the value, and only its `[Display]` attribute is used.
3. Members with `[Display(Order = ...)]` come first, sorted by `Order`. Members without `Order` come after all of them, even after `Order = int.MaxValue`. Members with the same `Order`, and members without one, keep the value order.

```csharp
public enum Priority
{
    Low = 1,
    [Display(Name = "High priority", Order = 0)]
    High = 3,
    Medium = 2,
    Normal = 2 // Alias of Medium. "Medium" sorts first, so it represents the value.
}

// Items: High priority, Low, Medium
var priority = Prompt.Select<Priority>("Select priority");
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
