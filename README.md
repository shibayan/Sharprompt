# Sharprompt

![Build](https://github.com/shibayan/Sharprompt/workflows/Build/badge.svg)
[![License](https://img.shields.io/github/license/shibayan/Sharprompt)](https://github.com/shibayan/Sharprompt/blob/master/LICENSE)
[![Downloads](https://img.shields.io/nuget/dt/Sharprompt)](https://www.nuget.org/packages/Sharprompt-FC/)

# Sharprompt-FC

[![License](https://img.shields.io/github/license/FRACerqueira/Sharprompt)](https://github.com/FRACerqueira/Sharprompt/blob/master/LICENSE)
[![Downloads](https://img.shields.io/nuget/dt/Sharprompt-FC)](https://www.nuget.org/packages/Sharprompt-FC/)

Interactive command line interface toolkit for C#

![sharprompt](https://user-images.githubusercontent.com/1356444/62227794-87506e00-b3f7-11e9-84ae-06c9a900448b.gif)

## NuGet Package
Package Name | Target Framework | NuGet
---|---|---
Sharprompt    | .NET Standard 2.0 | [![NuGet](https://img.shields.io/nuget/v/Sharprompt)](https://www.nuget.org/packages/Sharprompt/)
Sharprompt-FC | .NET Standard 2.0 | [![NuGet](https://img.shields.io/nuget/v/Sharprompt-FC)](https://www.nuget.org/packages/Sharprompt-FC/)

## Install

```
Install-Package Sharprompt-FC
```

```
dotnet add package Sharprompt-FC
```

## Features 

- Multi-platform support
- Supports the popular Prompts (Input / Password / Select / etc)
- Validation of input value
- Automatic generation of data source using Enum value
- Customize the color scheme
- Unicode support (East asian width and Emoji)

## Fork (2.3.0) with new features / enhancement  (2.4.0)

- Support to Cancelation Token for all promts (enhancement)
- Custom Prompter Messages (enhancement)
- Prompt Input start buffer with default value (enhancement)
- Prompt List with remove item (behavior macth all/one by listoption) (enhancement)
- Prompt Select/MultiSelect/List with Interative result (enhancement)
- Prompt Confirm start buffer with default value and supress message when Is Control key(enhancement)
- Prompt Confirm removed long answer (behavior change) 
- Prompt Anykey (new) 
- Prompt FileBrowser (preview - Browser file/folder) 

## Usage

```csharp
// Simple Input prompt
var name = Prompt.Input<string>("What's your name?");
Console.WriteLine($"Hello, {name}!");

// Password prompt
var secret = Prompt.Password("Type new password", new[] { Validators.Required(), Validators.MinLength(8) });
Console.WriteLine("Password OK");

// Confirmation prompt
var answer = Prompt.Confirm("Are you ready?", defaultValue: true);
Console.WriteLine($"Your answer is {answer}");

// Pause to key press
Prompt.AnyKey();
```

## APIs

### Input

```csharp
var name = Prompt.Input<string>("What's your name?");
Console.WriteLine($"Hello, {name}!");
```

![input](https://user-images.githubusercontent.com/1356444/62228275-50c72300-b3f8-11e9-8d51-63892e8eeaaa.gif)

### Cancel prompts
```csharp
using (var tokenSource = new CancellationTokenSource(5000))
{
    var name = Prompt.Input<string>("What's your name? - Hurry up!", tokenSource.Token);
    if (!tokenSource.Token.IsCancellationRequested)
    {
        Console.WriteLine($"Hello, {name}!");
    }
}
```

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

**Enum value support**

```csharp
var value = Prompt.Select<MyEnum>("Select enum value");
Console.WriteLine($"You selected {value}");
```

### MultiSelect

```csharp
var cities = Prompt.MultiSelect("Which cities would you like to visit?", new[] { "Seattle", "London", "Tokyo", "New York", "Singapore", "Shanghai" }, pageSize: 3);
Console.WriteLine($"You picked {string.Join(", ", options)}");
```

### FileBrowser

```csharp
var folder = Prompt.FileBrowser(FileBrowserChoose.Folder, "Select/New folder", _stopApp, pageSize: 5, promptCurrentPath: false);
if (!_stopApp.IsCancellationRequested)
{
    var dirfound = folder.NotFound ? "not found" : "found";
    Console.WriteLine($"You picked, {Path.Combine(folder.PathValue,folder.SelectedValue)} and {dirfound}");
}

var file = Prompt.FileBrowser(FileBrowserChoose.File, "Select/New file", _stopApp, pageSize: 10);
if (!_stopApp.IsCancellationRequested)
{
    if (string.IsNullOrEmpty(file.SelectedValue))
    {
        Console.WriteLine("You chose nothing!");
    }
    else
    {
        var filefound = file.NotFound ? "not found" : "found";
        Console.WriteLine($"You picked, {Path.Combine(file.PathValue, file.SelectedValue)} and {filefound}");
    }
}

```



## Configuration

### Custom Prompter

```csharp
Prompt.ColorSchema.Answer = ConsoleColor.DarkRed;
Prompt.ColorSchema.Select = ConsoleColor.DarkCyan;
Prompt.DefaultMessageValues.DefaultYesKey = 'Y';
Prompt.DefaultMessageValues.DefaultNoKey = 'N';

var name = Prompt.Input<string>("What's your name?");
Console.WriteLine($"Hello, {name}!");
```

### Unicode Support

![unicode](https://user-images.githubusercontent.com/1356444/89803983-86a3f900-db6e-11ea-8fc8-5b6f9ef5644f.gif)

```csharp
// Prefer UTF-8 as the output encoding
Console.OutputEncoding = Encoding.UTF8;

var name = Prompt.Input<string>("What's your name?");
Console.WriteLine($"Hello, {name}!");
```

## Platforms

- Windows
  - Command Prompt / PowerShell / Windows Terminal
- Ubuntu
  - Bash
- macOS
  - Bash

## License

This project is licensed under the [MIT License](https://github.com/shibayan/Sharprompt/blob/master/LICENSE)
