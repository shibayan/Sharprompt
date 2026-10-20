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

## Native AOT & Source Generators

Sharprompt is configured with `<IsAotCompatible>true</IsAotCompatible>` and supports Native AOT compilation on .NET 8 and .NET 10.

### How It Works

Sharprompt uses Roslyn Incremental Source Generators to generate code at compile time, eliminating runtime reflection:

1. **`[PromptBindable]` Generator**: Inspects classes and records annotated with `[PromptBindable]` and generates strongly typed model binding code registered via a C# `[ModuleInitializer]`. When invoking `Prompt.Bind<T>()`, Sharprompt uses the precompiled binder with zero reflection.
2. **Enum Metadata Generator**: Inspects enum declarations in your project and generates metadata (values, `[Display]` names, and ordering) registered via a `[ModuleInitializer]`.

### Project Configuration & Publishing

To publish an application with Native AOT, add `<PublishAot>true</PublishAot>` to your project file:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <PublishAot>true</PublishAot>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Sharprompt" Version="..." />
  </ItemGroup>

</Project>
```

Publish your application using the `dotnet publish` command targeting your runtime:

```sh
dotnet publish -c Release -r win-x64
```

Replace `win-x64` with your target runtime identifier (such as `linux-x64` or `osx-arm64`).

### Enum Handling & External Assemblies

- **Project Enums**: Any enum declared within the consuming project is automatically analyzed by the source generator at compile time. Display names and custom ordering specified with `[Display]` are generated directly into code.
- **External Enums**: Enums from external assemblies (such as BCL enums like `DayOfWeek` or enums from referenced packages) are not analyzed by the source generator. At runtime, Sharprompt falls back to an AOT-safe reflection mechanism. To avoid reflection entirely or customize the display, register the enum manually at startup:

```csharp
using Sharprompt;

EnumMetadataRegistry.Register<DayOfWeek>(
    Enum.GetValues<DayOfWeek>(),
    day => day.ToString()
);
```

### Type Conversion & Custom Types

Prompts rely on `TypeDescriptor` converters to parse user input. Built-in intrinsic converters (such as `string`, numeric types, `bool`, `char`, `Guid`, `DateTime`, and enums) are fully preserved under Native AOT.

If using custom types annotated with `[TypeConverter]`, ensure your application preserves the converter type from trimming (for example, using `[DynamicallyAccessedMembers]` or trimming configuration).

### Cancellation

All prompts, including model binding, support cancellation via `CancellationToken`. Pass the token to `Prompt.Bind`:

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

try
{
    var form = Prompt.Bind<MyFormModel>(cts.Token);
}
catch (OperationCanceledException)
{
    Console.WriteLine("Operation timed out or was canceled.");
}
```

If the token is already canceled, `Prompt.Bind` throws before any prompt is shown. Prompts wait for keys through `IConsoleDriver.ReadKey(CancellationToken)`, which custom console drivers can override when their input source supports cancellation natively. See [Canceling from code](/configuration#canceling-from-code) for details.
