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
- Bound properties must be public instance properties with accessible `get` and `set` accessors (`public`, `internal`, or `protected internal`). Read-only properties are skipped. Properties without an accessible getter are skipped with a `SHARP010` warning, because the current value is used as the default.
- Init-only properties are skipped with a `SHARP002` warning. This includes the properties generated from positional record parameters, so declare bound properties with `{ get; set; }` in the record body.
- Collection properties must be `T[]`, `List<T>`, `HashSet<T>`, `IEnumerable<T>`, or an interface one of them implements (such as `IList<T>` or `IReadOnlyCollection<T>`). Other collection types are skipped with a `SHARP003` warning.
- Properties bound with `Input` or `List` are converted from text, so their type must have a `TypeConverter`. See [Type Conversion & Custom Types](/advanced#type-conversion-custom-types) for the supported types.
- If the model has no property that can be bound, no binder is generated (`SHARP014`) and `Prompt.Bind` throws an `InvalidOperationException`.

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

The source generator reports these warnings for `[PromptBindable]` models, at the attribute or property involved. Instead of generating code that does not compile or that fails at runtime, it ignores the attribute or skips the property, and still binds the rest of the model.

| ID | Description |
|----|-------------|
| `SHARP001` | The member referenced by `[MemberItems]` is missing, or is not a public static property (with an accessible getter) or parameterless method. The items provider is ignored. |
| `SHARP002` | The property has an init-only setter and is skipped. Use a `set` accessor or apply `[BindIgnore]`. |
| `SHARP003` | The property's collection type is not supported and the property is skipped. Use a supported collection type or apply `[BindIgnore]`. |
| `SHARP004` | The model is generic, or nested in a generic type. No binder is generated. |
| `SHARP005` | The model, or a type containing it, is not accessible from other files in the assembly (such as a `private` nested or `file` type). No binder is generated. |
| `SHARP006` | The member referenced by `[MemberItems]` does not return an `IEnumerable<T>` of the property's item type. The items provider is ignored. |
| `SHARP007` | A value in `[InlineItems]` is `null` or not of the property's item type (for example `1` for a `long` property; write `1L`). The items provider is ignored. |
| `SHARP008` | An items attribute has no effect: `[MemberItems]` together with `[InlineItems]`, or items on a property bound with Confirm (`bool`) or Password. The attribute is ignored. |
| `SHARP009` | `[DataType(DataType.Password)]` is applied to a property that is not a `string`. The property is skipped. |
| `SHARP010` | The property has no `public` or `internal` getter. The property is skipped. |
| `SHARP011` | The property's type is a ref struct (such as `Span<T>`) or a pointer, which prompts cannot return. The property is skipped. |
| `SHARP012` | The property is read as text, but its type has no `TypeConverter` that converts from string, so every input would be rejected. The property is still bound, in case a converter is registered at runtime. |
| `SHARP013` | A validation attribute, or a type in its arguments, is not accessible from other files in the assembly. The validator is skipped. |
| `SHARP014` | The model has no property that can be bound. No binder is generated, and `Prompt.Bind` throws. |

Each warning can be suppressed with `#pragma warning disable` or in `.editorconfig`, like any other compiler warning.

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
