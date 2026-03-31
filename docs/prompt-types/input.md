# Input

The `Input` prompt takes a generic type parameter and performs type conversion as appropriate.

## Basic Usage

```csharp
var name = Prompt.Input<string>("What's your name?");
Console.WriteLine($"Hello, {name}!");
```

You can also use it with other types:

```csharp
var number = Prompt.Input<int>("Enter any number");
Console.WriteLine($"Input = {number}");
```

## Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `message` | `string` | The message to display to the user |
| `defaultValue` | `object?` | Default value if the user presses Enter without input |
| `placeholder` | `string?` | Placeholder text displayed in the input area |
| `validators` | `IList<Func<object?, ValidationResult?>>?` | List of validators to apply to the input |

## Options Class

You can use `InputOptions<T>` for more control:

```csharp
var name = Prompt.Input(new InputOptions<string>
{
    Message = "What's your name?",
    DefaultValue = "John Smith",
    Placeholder = "At least 3 characters",
    Validators = { Validators.Required(), Validators.MinLength(3) }
});
```

## Fluent API

```csharp
using Sharprompt.Fluent;

var name = Prompt.Input<string>(o => o.WithMessage("What's your name?")
                                      .WithDefaultValue("John Smith")
                                      .WithPlaceholder("At least 3 characters")
                                      .AddValidators(Validators.Required(), Validators.MinLength(3)));
```

## With Validation

```csharp
var name = Prompt.Input<string>("What's your name?",
    defaultValue: "John Smith",
    placeholder: "At least 3 characters",
    validators: new[] { Validators.Required(), Validators.MinLength(3) });
```
