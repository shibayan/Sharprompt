# Sharprompt
 Interactive command line interface tookit for .NET Core

![sharprompt](https://user-images.githubusercontent.com/1356444/62069893-43812b80-b274-11e9-83d6-150f716ec4bd.gif)

## Install

```
Install-Package Sharprompt -Version 1.0.0-preview
```

```
dotnet add package Sharprompt --version 1.0.0-preview
```

## Usage

```csharp
var name = Prompt.Input<string>("Name");
Console.WriteLine($"Hello, {name}!");
```

## APIs

### Input

```csharp
var name = Prompt.Input<string>("Name");
Console.WriteLine($"Hello, {name}!");
```

### Confirm

```csharp
var answer = Prompt.Confirm("Are you ready?");
Console.WriteLine($"Your answer is {answer}");
```

### Password

```csharp
var secret = Prompt.Password("Type new password");
Console.WriteLine("Password OK");
```

### Select

```csharp
var city = Prompt.Select("Select your city", new[] { "Seattle", "London", "Tokyo" });
Console.WriteLine($"Hello, {city}!");
```