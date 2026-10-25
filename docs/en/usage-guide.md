# Usage Guide

Sharprompt is a synchronous library for interactive C# console applications on .NET 8 and .NET 10. Install the `Sharprompt` NuGet package and import `Sharprompt`. The package includes the source generators and XML API documentation; no separate generator package is needed.

## Choose a Prompt

| Task | API | Result and important defaults |
| --- | --- | --- |
| Read a typed value | `Prompt.Input<T>(message, ...)` | A value converted with an invariant-culture `TypeConverter` |
| Read a secret | `Prompt.Password(message, ...)` | A masked input returned as a plain `string`; do not log it |
| Ask yes or no | `Prompt.Confirm(message, defaultValue: ...)` | `bool`; without a default, requires an explicit answer |
| Choose one item | `Prompt.Select(message, items, ...)` | The original item, not its display text |
| Choose several items | `Prompt.MultiSelect(message, items, ...)` | `IEnumerable<T>`; at least one item by default; `minimum: 0` permits none |
| Enter a list | `Prompt.List<T>(message, ...)` | `IEnumerable<T>`; an empty line finishes after `minimum` items (default: 1) |
| Populate a model | `Prompt.Bind<T>()` or `Prompt.Bind(model)` | A model populated by a source-generated binder |

Each prompt except `Bind` offers three styles: message arguments, an options instance, or an `Action<Options>` configure delegate. Prefer the message overload for simple calls and an options/configure overload for advanced settings or cancellation. The fluent extensions require `using Sharprompt.Fluent;`.

## A Complete Program

Create a console application with `dotnet new console`, add the package with `dotnet add package Sharprompt`, and replace `Program.cs` with:

```csharp
using Sharprompt;

var name = Prompt.Input<string>("Name", validators: new[] { Validators.Required() });
var city = Prompt.Select("City", new[] { "Tokyo", "London" });
var ready = Prompt.Confirm("Continue?", defaultValue: true);
Console.WriteLine($"{name}, {city}, ready: {ready}");
```

Run this program in an interactive terminal. For unattended verification, use the [testing guide](/testing).

## Defaults and Validation

- Use defaults compatible with `T`; compatible primitive conversions and integral enum values are also accepted. For `Input<T>`, Enter accepts the default, Tab inserts it for editing, and Backspace with empty input dismisses it.
- `Select<T>` and `MultiSelect<T>` can omit `items` for enums. For other types, supply items. `textSelector` changes display and filtering, while the returned objects remain the original items.
- Input conversion uses invariant culture, independently of `Prompt.Culture`. For example, use `1.5` for a decimal value.
- Validators return `ValidationResult.Success` (`null`) to accept a value. A `ValidationResult` displays an error and keeps the prompt open. `Required()` rejects null and empty strings, but accepts whitespace. Length and regex validators accept non-string values. `RegularExpression()` matches substrings unless the pattern is anchored.

See [validators](/validators) and [type conversion](/advanced#type-conversion-custom-types).

## Model Binding Requirements

Annotate the model with `[PromptBindable]`. Use a class or record class with public instance properties and accessible `get`/`set` accessors. `Prompt.Bind<T>()` requires a public parameterless constructor; `Prompt.Bind(model)` accepts an existing instance.

Use `{ get; set; }` properties in the body of records. Positional record properties and other init-only properties are skipped (`SHARP002`). Generic models are unsupported. If no properties can be bound, no binder is generated (`SHARP014`). There is no runtime reflection binder fallback.

Keep top-level calls before type declarations in a single `Program.cs`. See [Bind](/prompt-types/bind) for complete examples, supported collections, and the `SHARP001`–`SHARP014` diagnostics with fixes.

## Cancellation and Automation

The default driver requires terminal input and output. Redirecting either stream throws `InvalidOperationException`; piping text into `dotnet run` does not simulate key input. Use `Prompt.ConsoleDriverFactory` with a scripted driver for [automated tests](/testing). Applications can also accept command-line arguments and skip prompts when values are provided; Sharprompt itself does not parse arguments.

By default Ctrl+C and Escape exit the process with code 1. Set `Prompt.ThrowExceptionOnCancel = true` to receive `PromptCanceledException`. A `CancellationToken` instead throws `OperationCanceledException`, regardless of that setting. Pass it through an options/configure overload, such as `Prompt.Confirm(options => options.Message = "Continue?", token)`; the message overload has no token parameter. `Prompt.Bind<T>(token)` is also supported.

Configuration is process-wide. Restore changed settings in `finally`, and serialize tests that modify them. See [configuration](/configuration) for full cancellation examples and custom driver requirements, and [Native AOT](/advanced#native-aot-source-generators) for publishing guidance.
