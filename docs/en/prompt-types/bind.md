# Bind (Model-based Prompts)

The `Bind` prompt automatically generates a sequence of prompts based on a model class or record.

## Basic Usage

Define a model class (or record) with `[PromptBindable]` and data annotations:

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
- The model must be declared as a `class` or a `record` (`record class`). Structs, including `record struct`, are not supported.
- The model must have the `[PromptBindable]` attribute so the Roslyn source generator generates a binder at compile time.
- The model must have a public parameterless constructor when using `Prompt.Bind<T>()`. Alternatively, pass an existing instance to `Prompt.Bind(model)`.
- Bound properties must be public instance properties with accessible `set` accessors (`public`, `internal`, or `protected internal`). Read-only properties are skipped.
- Init-only properties are skipped with a `SHARP002` warning. This includes the properties generated from positional record parameters, so declare bound properties with `{ get; set; }` in the record body.
- Collection properties must be `T[]`, `List<T>`, `HashSet<T>`, `IEnumerable<T>`, or an interface one of them implements (such as `IList<T>` or `IReadOnlyCollection<T>`). Other collection types are skipped with a `SHARP003` warning.

```csharp
[PromptBindable]
public record MyRecordModel
{
    [Display(Name = "What's your name?")]
    public string Name { get; set; } = "";
}
```

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
| `[PromptBindable]` | Marks the class or record for source-generated model binding |
| `[Display(Name = "...")]` | Sets the prompt message (`Name`) and placeholder (`Prompt`) |
| `[Display(Order = n)]` | Sets the prompt order. Properties with `Order` are prompted in ascending order; properties without it are prompted after every ordered property, in declaration order |
| `[Required]` | Makes the field required |
| `[MinLength(n)]` | Sets minimum length validation |
| `[MaxLength(n)]` | Sets maximum length validation |
| `[DataType(DataType.Password)]` | Uses password prompt |
| `[InlineItems("a", "b", "c")]` | Provides static inline items for selection |
| `[MemberItems(nameof(Member))]` | Provides items dynamically from a public static property or parameterless method of the model (or of the type passed as the second argument) |
| `[BindIgnore]` | Skips the property during binding |

## Compiler Diagnostics

The source generator reports these warnings for `[PromptBindable]` models:

| ID | Description |
|----|-------------|
| `SHARP001` | The member referenced by `[MemberItems]` is missing, or is not a public static property or parameterless method. The items provider is ignored. |
| `SHARP002` | The property has an init-only setter and is skipped. Use a `set` accessor or apply `[BindIgnore]`. |
| `SHARP003` | The property's collection type is not supported and the property is skipped. Use a supported collection type or apply `[BindIgnore]`. |

## Cancellation

You can pass a `CancellationToken` to cancel model binding. If the token is canceled while a prompt is waiting for input, that prompt is closed, the remaining properties are not prompted, and an `OperationCanceledException` is thrown. If the token is already canceled when `Prompt.Bind` is called, the exception is thrown before any prompt is shown:

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

See [Canceling from code](/configuration#canceling-from-code) for details, including how custom console drivers can override `IConsoleDriver.ReadKey(CancellationToken)`.
