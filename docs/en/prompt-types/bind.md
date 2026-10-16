# Bind (Model-based Prompts)

The `Bind` prompt automatically generates a sequence of prompts based on a model class.

## Basic Usage

Define a model class with `[PromptBindable]` and data annotations:

```csharp
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Sharprompt;

[PromptBindable]
public class MyFormModel
{
    [Display(Name = "What's your name?")]
    [Required]
    public string Name { get; set; } = null!;

    [Display(Name = "Type new password")]
    [DataType(DataType.Password)]
    [Required]
    [MinLength(8)]
    public string Password { get; set; } = null!;

    [Display(Name = "Select your city")]
    [Required]
    [InlineItems("Seattle", "London", "Tokyo")]
    public string City { get; set; } = null!;

    [Display(Name = "Select favorite fruit")]
    [MemberItems(nameof(Fruits))]
    public string Fruit { get; set; } = null!;

    [Display(Name = "Are you ready?")]
    public bool? Ready { get; set; }

    public static IEnumerable<string> Fruits => new[] { "Apple", "Banana", "Orange" };
}
```

Then use `Prompt.Bind` to generate prompts for the model:

```csharp
var result = Prompt.Bind<MyFormModel>();
```

## Model Requirements

To work with source-generated binding (including Native AOT):
- The model must be declared as a `class`.
- The class must have the `[PromptBindable]` attribute so the Roslyn source generator generates a binder at compile time.
- The class must have a public parameterless constructor when using `Prompt.Bind<T>()`. Alternatively, pass an existing instance to `Prompt.Bind(model)`.
- Bound properties must be public instance properties with accessible setters (`public`, `internal`, or `protected internal`). Init-only or read-only properties are skipped.

## How It Works

Sharprompt automatically determines the prompt type based on property types and attributes:

| Property Type / Attribute | Prompt Type |
|--------------------------|-------------|
| `string` | Input |
| `string` with `[DataType(DataType.Password)]` | Password |
| `bool` or `bool?` | Confirm |
| Single value with `[InlineItems(...)]` or `[MemberItems(...)]` | Select |
| `IEnumerable<T>` with `[InlineItems(...)]` or `[MemberItems(...)]` | MultiSelect |
| Enum type | Select (or MultiSelect if collection) |
| `IEnumerable<T>` (without items attribute) | List |

## Supported Attributes

| Attribute | Description |
|-----------|-------------|
| `[PromptBindable]` | Marks the class for source-generated model binding |
| `[Display(Name = "...")]` | Sets prompt message (`Name`), placeholder (`Prompt`), and order (`Order`) |
| `[Required]` | Makes the field required |
| `[MinLength(n)]` | Sets minimum length validation |
| `[MaxLength(n)]` | Sets maximum length validation |
| `[DataType(DataType.Password)]` | Uses password prompt |
| `[InlineItems("a", "b", "c")]` | Provides static inline items for selection |
| `[MemberItems(nameof(Member))]` | Provides items dynamically from a static property or method |
| `[BindIgnore]` | Skips the property during binding |

## Cancellation

You can pass a `CancellationToken` to cancel model binding at any point:

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

try
{
    var result = Prompt.Bind<MyFormModel>(cts.Token);
}
catch (OperationCanceledException)
{
    Console.WriteLine("Prompt binding was canceled.");
}
```
