# Validators

Sharprompt provides built-in validators that can be used with the `validators` parameter on `Input`, `Password`, and `List` prompts.

## Usage

```csharp
var secret = Prompt.Password("Type new password",
    validators: new[] { Validators.Required(), Validators.MinLength(8) });
```

## Built-in Validators

| Validator | Description |
|-----------|-------------|
| `Validators.Required()` | Ensures the input is not empty |
| `Validators.MinLength(length)` | Ensures the input has at least the specified number of characters |
| `Validators.MaxLength(length)` | Ensures the input does not exceed the specified number of characters |
| `Validators.RegularExpression(pattern)` | Ensures the input matches the specified regular expression |

## Combining Validators

You can combine multiple validators:

```csharp
var name = Prompt.Input<string>("What's your name?",
    validators: new[]
    {
        Validators.Required(),
        Validators.MinLength(3),
        Validators.MaxLength(50)
    });
```

## Custom Validators

Validators are functions that return a `ValidationResult`. You can create custom validators:

```csharp
var email = Prompt.Input<string>("Enter your email",
    validators: new Func<object?, ValidationResult?>[]
    {
        Validators.Required(),
        input =>
        {
            var value = input?.ToString();
            if (value != null && !value.Contains('@'))
            {
                return new ValidationResult("Please enter a valid email address");
            }
            return ValidationResult.Success;
        }
    });
```
