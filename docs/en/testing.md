# Automated Testing

The default console driver rejects redirected standard input or output. For CI or coding-agent verification, inject an `IConsoleDriver` using `Prompt.ConsoleDriverFactory`. This exercises the public prompts and source-generated binding without a terminal.

## Run the Complete Example

From the repository root:

```sh
dotnet run --project samples/Sharprompt.Automation -f net8.0
dotnet run --project samples/Sharprompt.Automation -f net10.0
```

The example checks Input, Select, MultiSelect, Confirm, Password, List, validation retries, model binding, and cancellation after a prompt starts. An unexpected answer or exhausted input fails immediately. CI runs both frameworks. The following source is imported from that compiled sample.

## Program.cs

<<< @/../samples/Sharprompt.Automation/Program.cs

## ScriptedConsoleDriver.cs

<<< @/../samples/Sharprompt.Automation/ScriptedConsoleDriver.cs

## Adapting It to Your Tests

- Save and restore `ConsoleDriverFactory` and any other changed `Prompt` settings in `try`/`finally`. They are process-wide. Disable parallel execution for tests sharing these settings.
- Each prompt owns and disposes its driver. Return a fresh driver for each prompt, including each property in `Bind`. Use a queue of driver scripts for models with different property inputs.
- Set `ThrowExceptionOnCancel = true` when testing Escape or a driver that invokes `CancellationCallback`; otherwise user cancellation exits the process. Token cancellation always throws `OperationCanceledException`.
- The sample overrides `ReadKey(CancellationToken)` to avoid waiting and to fail on exhausted input. If relying on the interface's default implementation, `KeyAvailable` must accurately report queued keys or a cancelable prompt can wait forever.
- This small driver supports short ASCII scenarios and records writes rather than a final terminal screen. It does not emulate ANSI styles, Unicode column widths, wrapping, or scrolling. Use a fuller terminal emulator for rendering assertions.

## README Examples

`dotnet test -c Release` also compiles every C# block in the README as a separate console program with the standard console-project implicit usings. Both source generators run during this check. The tests do not launch those interactive programs. This catches API drift and invalid examples, including top-level statements placed after type declarations.
