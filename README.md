# Sharprompt

[![Build](https://github.com/shibayan/Sharprompt/workflows/Build/badge.svg)](https://github.com/shibayan/Sharprompt/actions/workflows/build.yml)
[![Downloads](https://badgen.net/nuget/dt/Sharprompt)](https://www.nuget.org/packages/Sharprompt/)
[![NuGet](https://badgen.net/nuget/v/Sharprompt)](https://www.nuget.org/packages/Sharprompt/)
[![License](https://badgen.net/github/license/shibayan/Sharprompt)](https://github.com/shibayan/Sharprompt/blob/master/LICENSE)

Interactive command-line based application framework for C#

![sharprompt](https://user-images.githubusercontent.com/1356444/62227794-87506e00-b3f7-11e9-84ae-06c9a900448b.gif)

## Features

- Multi-platform support
- Supports the popular prompts (`Input` / `Password` / `Select` / etc)
- Supports model-based prompts
- Validation of input value
- Automatic generation of data source using Enum type
- Customizable symbols and color schema
- Unicode support (Multi-byte characters and Emoji😀🎉)

## Installation

```
Install-Package Sharprompt
```

```
dotnet add package Sharprompt
```

```csharp
// Simple input
var name = Prompt.Input<string>("What's your name?");
Console.WriteLine($"Hello, {name}!");

// Password input
var secret = Prompt.Password("Type new password", validators: new[] { Validators.Required(), Validators.MinLength(8) });
Console.WriteLine("Password OK");

// Confirmation
var answer = Prompt.Confirm("Are you ready?", defaultValue: true);
Console.WriteLine($"Your answer is {answer}");
```

## Examples

The project in the folder `Sharprompt.Example` contains all the samples. Please check it.

```
dotnet run --project Sharprompt.Example
```

## Prompt types

### Input

Takes a generic type parameter and performs type conversion as appropriate.

```csharp
var name = Prompt.Input<string>("What's your name?");
Console.WriteLine($"Hello, {name}!");

var number = Prompt.Input<int>("Enter any number");
Console.WriteLine($"Input = {number}");
```

![input](https://user-images.githubusercontent.com/1356444/62228275-50c72300-b3f8-11e9-8d51-63892e8eeaaa.gif)

### Confirm

```csharp
var answer = Prompt.Confirm("Are you ready?");
Console.WriteLine($"Your answer is {answer}");
```

![confirm](https://user-images.githubusercontent.com/1356444/62229064-e0210600-b3f9-11e9-8c52-b9c9257811c0.gif)

### Password

```csharp
var secret = Prompt.Password("Type new password");
Console.WriteLine("Password OK");
```

![password](https://user-images.githubusercontent.com/1356444/62228952-9fc18800-b3f9-11e9-98ea-3aa52ee84e93.gif)

### Select

```csharp
var city = Prompt.Select("Select your city", new[] { "Seattle", "London", "Tokyo" });
Console.WriteLine($"Hello, {city}!");
```

![select](https://user-images.githubusercontent.com/1356444/62228719-2de93e80-b3f9-11e9-8be5-f19e6ef58aeb.gif)

### MultiSelect (Checkbox)

```csharp
var cities = Prompt.MultiSelect("Which cities would you like to visit?", new[] { "Seattle", "London", "Tokyo", "New York", "Singapore", "Shanghai" }, pageSize: 3);
Console.WriteLine($"You picked {string.Join(", ", cities)}");
```

![multiselect](https://user-images.githubusercontent.com/1356444/127033929-3278e39c-e260-4aed-9c3c-3cfd7d3f3549.gif)

### List

```csharp
var value = Prompt.List<string>("Please add item(s)");
Console.WriteLine($"You picked {string.Join(", ", value)}");
```

![list](https://user-images.githubusercontent.com/1356444/127033968-cf70bd1b-bcd1-4c4f-bdbe-74aae52cdb86.gif)

### Bind (Model-based prompts)

```csharp
// Input model definition
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

var result = Prompt.Bind<MyFormModel>();
```

## Configuration

### Symbols

```csharp
Prompt.Symbols.Prompt = new Symbol("🤔", "?");
Prompt.Symbols.Done = new Symbol("😎", "V");
Prompt.Symbols.Error = new Symbol("😱", ">>");

var name = Prompt.Input<string>("What's your name?");
Console.WriteLine($"Hello, {name}!");
```

### Color schema

```csharp
Prompt.ColorSchema.Answer = ConsoleColor.DarkRed;
Prompt.ColorSchema.Select = ConsoleColor.DarkCyan;

var name = Prompt.Input<string>("What's your name?");
Console.WriteLine($"Hello, {name}!");
```

### Cancellation support

```csharp
// Throw an exception when canceling with Ctrl-C
Prompt.ThrowExceptionOnCancel = true;

try
{
    var name = Prompt.Input<string>("What's your name?");
    Console.WriteLine($"Hello, {name}!");
}
catch (PromptCanceledException ex)
{
    Console.WriteLine("Prompt canceled");
}
```

## Features

### Enum type support

```csharp
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

### Unicode support

```csharp
// Prefer UTF-8 as the output encoding
Console.OutputEncoding = Encoding.UTF8;

var name = Prompt.Input<string>("What's your name?");
Console.WriteLine($"Hello, {name}!");
```

![unicode support](https://user-images.githubusercontent.com/1356444/89803983-86a3f900-db6e-11ea-8fc8-5b6f9ef5644f.gif)

### Fluent interface support

```csharp
using Sharprompt.Fluent;

// Use fluent interface
var city = Prompt.Select<string>(o => o.WithMessage("Select your city")
                                       .WithItems(new[] { "Seattle", "London", "Tokyo" })
                                       .WithDefaultValue("Seattle"));
```

## Supported platforms

- Windows
  - Command Prompt
  - PowerShell
  - Windows Terminal
- Linux (Ubuntu, etc)
  - Windows Terminal (WSL 2)
- macOS
  - Terminal.app

## License

This project is licensed under the [MIT License](https://github.com/shibayan/Sharprompt/blob/master/LICENSE)
