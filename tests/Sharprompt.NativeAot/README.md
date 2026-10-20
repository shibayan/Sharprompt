# Native AOT verification

Verifies that an application consuming the **Sharprompt NuGet package** can be published with Native AOT and run as a native binary. Compatibility declarations such as `IsAotCompatible` only cover analyzer warnings inside this repository. This check covers what users get: the packed library, its satellite resources, and the source generators shipped in the package.

The `native-aot` job in [build.yml](../../.github/workflows/build.yml) runs it on every push and pull request.

## What it does

[verify.ps1](verify.ps1):

1. Packs `src/Sharprompt` into `artifacts/packages` with a unique prerelease version.
2. For each target framework, publishes `Sharprompt.NativeAot.App` with `PublishAot` against that package.
   - [nuget.config](nuget.config) maps the `Sharprompt` package ID to the local folder only, so the published version on nuget.org is never used by mistake.
   - `TreatWarningsAsErrors` and `TrimmerSingleWarn=false` fail the publish on any trim or AOT warning, including warnings inside the Sharprompt package.
3. Checks the generated sources to confirm that the packaged source generators ran in the consuming project.
4. Runs the native binary. A scripted `IConsoleDriver` replays key presses one at a time, so the prompt renders after every key as it does with interactive typing. The process exits with a non-zero code when any scenario fails.

## Coverage

| Area | Scenarios |
| --- | --- |
| `Input<T>` conversion | Every type listed as supported in the [Type Conversion & Custom Types](../../docs/en/advanced.md#type-conversion--custom-types) documentation, enums from both metadata paths, and custom types with `[TypeConverter]`. `Temperature` is declared in the app, and `ExternalTemperature` in the trimmable `Sharprompt.NativeAot.External` library. |
| `Input<T>` behavior | Empty input for nullable types, conversion errors, default values, editing the default value with Tab (formatting and parsing it again), and showing default values and answers in the invariant format under a non-invariant culture |
| Other prompts | `Password`, `Confirm`, `Select`, `MultiSelect`, and `List` |
| Enum metadata | `LocalPriority`, declared in the app, uses the generated metadata. `ExternalPriority`, declared in a separate assembly that does not reference Sharprompt, and `DayOfWeek` use the reflection fallback. Both custom enums declare the same aliases, `Display.Name`, and `Display.Order`, and must produce the same items in the same order. `LocalTimeOfDay` and `ExternalTimeOfDay` read their names from a `Display.ResourceType` on each path, and must follow a change of the UI culture. |
| `Bind<T>` | A `[PromptBindable]` model with `[Required]`, `[StringLength]`, `[Range]`, `[DataType(DataType.Password)]`, `[InlineItems]`, enums from both metadata paths, an array, a `List<T>`, and a `bool` |
| Localization | Japanese (`ja`) resources for validation messages and hints |
| Cancellation | Escape with `ThrowExceptionOnCancel`, and a canceled `CancellationToken` passed to `Bind` |

CI runs on `ubuntu-latest` (`linux-x64`), `windows-latest` (`win-x64`), and `macos-latest` (`osx-arm64`), with the .NET 10 SDK, for both `net8.0` and `net10.0`.

## Running locally

Install the [Native AOT prerequisites](https://learn.microsoft.com/dotnet/core/deploying/native-aot/#prerequisites) for your platform and PowerShell 7.4 or later, then run from the repository root:

```sh
pwsh ./tests/Sharprompt.NativeAot/verify.ps1
```

To check a single target framework:

```sh
pwsh ./tests/Sharprompt.NativeAot/verify.ps1 -Framework net10.0
```

These projects are not part of `Sharprompt.slnx`. `verify.ps1` passes `SharpromptVersion` so that the app restores the packed package. Without it, for example when the app is opened in an IDE or restored by dependency submission, the app references the Sharprompt source projects instead, so restoring it still succeeds. That build does not verify the package, so always use `verify.ps1` for verification.

## Adding a scenario

Add a `runner.Run(...)` call to [Program.cs](Sharprompt.NativeAot.App/Program.cs). Script the keys on the driver first, then call the prompt and check the result with `Check`. A scenario fails if it leaves scripted keys unconsumed, or if a prompt waits for more keys than were scripted.
