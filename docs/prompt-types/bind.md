# Bind (Model-based Prompts)

The `Bind` prompt automatically generates prompts based on a model class using data annotations.

## Basic Usage

Define a model class with data annotations:

```csharp
using System.ComponentModel.DataAnnotations;

public class MyFormModel
{
    [Display(Name = "What's your name?")]
    [Required]
    public string Name { get; set; }

    [Display(Name = "Type new password")]
    [DataType(DataType.Password)]
    [Required]
    [MinLength(8)]
    public string Password { get; set; }

    [Display(Name = "Select your city")]
    [Required]
    [InlineItems("Seattle", "London", "Tokyo")]
    public string City { get; set; }

    [Display(Name = "Are you ready?")]
    public bool? Ready { get; set; }
}
```

Then use `Prompt.Bind` to generate prompts for the model:

```csharp
var result = Prompt.Bind<MyFormModel>();
```

## How It Works

Sharprompt automatically determines the prompt type based on the property type and attributes:

| Property Type / Attribute | Prompt Type |
|--------------------------|-------------|
| `string` | Input |
| `string` with `[DataType(DataType.Password)]` | Password |
| `bool?` | Confirm |
| `string` with `[InlineItems(...)]` | Select |
| Enum type | Select |

## Supported Attributes

| Attribute | Description |
|-----------|-------------|
| `[Display(Name = "...")]` | Sets the prompt message |
| `[Required]` | Makes the field required |
| `[MinLength(n)]` | Sets minimum length validation |
| `[MaxLength(n)]` | Sets maximum length validation |
| `[DataType(DataType.Password)]` | Uses password prompt |
| `[InlineItems("a", "b", "c")]` | Provides items for select prompt |
| `[BindIgnore]` | Skips the property during binding |
