# Password

The `Password` prompt accepts user input while masking the characters.

## Basic Usage

```csharp
var secret = Prompt.Password("Type new password");
Console.WriteLine("Password OK");
```

## Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `message` | `string` | The message to display to the user |
| `passwordChar` | `string` | Character used to mask input (default: `"*"`) |
| `placeholder` | `string?` | Placeholder text displayed in the input area |
| `validators` | `IList<Func<object?, ValidationResult?>>?` | List of validators to apply to the input |

## Options Class

```csharp
var secret = Prompt.Password(new PasswordOptions
{
    Message = "Type new password",
    Placeholder = "At least 8 characters",
    PasswordChar = "*",
    Validators = { Validators.Required(), Validators.MinLength(8) }
});
```

## Fluent API

```csharp
using Sharprompt.Fluent;

var secret = Prompt.Password(o => o.WithMessage("Type new password")
                                   .WithPlaceholder("At least 8 characters")
                                   .AddValidators(Validators.Required(), Validators.MinLength(8)));
```

## With Validation

```csharp
var secret = Prompt.Password("Type new password",
    placeholder: "At least 8 characters",
    validators: new[] { Validators.Required(), Validators.MinLength(8) });
```
