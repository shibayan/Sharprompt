using System;
using System.Globalization;
using System.Threading;

using Sharprompt;
using Sharprompt.NativeAot;
using Sharprompt.NativeAot.ExternalEnums;

// Fix the culture so the assertions do not depend on the machine running them.
CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

Prompt.ThrowExceptionOnCancel = true;

var runner = new ScenarioRunner();

// Input: conversion of typed text goes through TypeDescriptor converters.
InputConverts("string", "Sharprompt", "Sharprompt");
InputConverts("int", "42", 42);
InputConverts("long?", "9000000000", (long?)9_000_000_000);
InputConverts("double", "1.5", 1.5);
InputConverts("decimal", "0.1", 0.1m);
InputConverts("bool", "true", true);
InputConverts("char", "x", 'x');
InputConverts("Guid", "8f2c1b9e-3d4a-4e5f-9a6b-7c8d9e0f1a2b", new Guid("8f2c1b9e-3d4a-4e5f-9a6b-7c8d9e0f1a2b"));
InputConverts("DateTime", "2026-10-21T09:30:00", new DateTime(2026, 10, 21, 9, 30, 0));
InputConverts("DateTimeOffset", "2026-10-21T09:30:00+09:00", new DateTimeOffset(2026, 10, 21, 9, 30, 0, TimeSpan.FromHours(9)));
InputConverts("TimeSpan", "01:02:03", new TimeSpan(1, 2, 3));
InputConverts("LocalPriority", "High", LocalPriority.High);
InputConverts("ExternalPriority", "Medium", ExternalPriority.Medium);

runner.Run("Input<int?> returns null for empty input", driver =>
{
    driver.Enter();

    Check.Equal(null, Prompt.Input<int?>("Value"));
});

runner.Run("Input<int> rejects text it cannot convert", driver =>
{
    driver.Type("abc").Enter().Backspace(3).Type("7").Enter();

    Check.Equal(7, Prompt.Input<int>("Value"));
});

runner.Run("Input<int> returns the default value on Enter", driver =>
{
    driver.Enter();

    Check.Equal(5, Prompt.Input<int>("Value", defaultValue: 5));
    Check.Contains("(5)", driver.Output);
});

// Tab inserts the default value as text, so formatting and parsing it again must round-trip.
EditsDefaultValue("double", 1.5);
EditsDefaultValue("decimal", 12.34m);
EditsDefaultValue("DateTime", new DateTime(2026, 10, 21, 9, 30, 0));
EditsDefaultValue("LocalPriority", LocalPriority.Medium);

runner.Run("Input runs Validators.Required", driver =>
{
    driver.Enter().Type("ok").Enter();

    Check.Equal("ok", Prompt.Input<string>("Value", validators: [Validators.Required()]));
    Check.Contains("Value is required", driver.Output);
});

runner.Run("Password masks the typed text", driver =>
{
    driver.Type("hunter2").Enter();

    Check.Equal("hunter2", Prompt.Password("Password"));
    Check.DoesNotContain("hunter2", driver.Output);
});

runner.Run("Confirm accepts y", driver =>
{
    driver.Type("y").Enter();

    Check.Equal(true, Prompt.Confirm("Continue?"));
});

runner.Run("Confirm returns the default value on Enter", driver =>
{
    driver.Enter();

    Check.Equal(false, Prompt.Confirm("Continue?", defaultValue: false));
});

runner.Run("Select<string> returns the highlighted item", driver =>
{
    driver.Down(2).Enter();

    Check.Equal("banana", Prompt.Select("Fruit", ["apple", "banana", "cherry"]));
});

// Both enums declare the same members, so both metadata paths must list the same items in the same order.
SelectsEnum("generated metadata", LocalPriority.Low);
SelectsEnum("reflection fallback", ExternalPriority.Low);

runner.Run("Select<DayOfWeek> lists a BCL enum through the reflection fallback", driver =>
{
    driver.Down(2).Enter();

    Check.Equal(DayOfWeek.Monday, Prompt.Select<DayOfWeek>("Day"));
});

runner.Run("MultiSelect<string> returns the checked items", driver =>
{
    driver.Down().Space().Down().Space().Enter();

    Check.SequenceEqual(["apple", "banana"], Prompt.MultiSelect("Fruits", ["apple", "banana", "cherry"]));
});

MultiSelectsAllEnumItems("generated metadata", [LocalPriority.High, LocalPriority.Medium, LocalPriority.Low]);
MultiSelectsAllEnumItems("reflection fallback", [ExternalPriority.High, ExternalPriority.Medium, ExternalPriority.Low]);

runner.Run("List<int> converts each entry", driver =>
{
    driver.Type("1").Enter().Type("2").Enter().Enter();

    Check.SequenceEqual([1, 2], Prompt.List<int>("Numbers"));
});

runner.Run("Bind<SignupModel> uses the generated binder and validation attributes", driver =>
{
    driver.Enter().Type("Ada").Enter();                     // Name: [Required] rejects empty input
    driver.Type("200").Enter().Backspace(3).Type("36").Enter(); // Age: [Range(0, 150)] rejects 200
    driver.Type("hunter2").Enter();                         // Secret: [DataType(DataType.Password)]
    driver.Enter();                                         // Priority: the default value is preselected
    driver.Enter();                                         // ExternalPriority: the default value is preselected
    driver.Down().Space().Enter();                          // Languages: [InlineItems]
    driver.Type("aot").Enter().Enter();                     // Tags
    driver.Type("y").Enter();                               // Subscribe

    var model = Prompt.Bind<SignupModel>();

    Check.Equal("Ada", model.Name);
    Check.Equal(36, model.Age);
    Check.Equal("hunter2", model.Secret);
    Check.Equal(LocalPriority.Low, model.Priority);
    Check.Equal(ExternalPriority.Low, model.ExternalPriority);
    Check.SequenceEqual(["C#"], model.Languages);
    Check.SequenceEqual(["aot"], model.Tags);
    Check.Equal(true, model.Subscribe);
    Check.Contains("The Name field is required.", driver.Output);
    Check.Contains("The field Age must be between 0 and 150.", driver.Output);
    Check.DoesNotContain("hunter2", driver.Output);
});

runner.Run("Japanese resources are used for validation messages", driver =>
{
    Prompt.Culture = new CultureInfo("ja-JP");

    driver.Enter().Type("1").Enter();

    Check.Equal(1, Prompt.Input<int>("値"));
    Check.Contains("値が必須です", driver.Output);
});

runner.Run("Japanese resources are used for hints", driver =>
{
    Prompt.Culture = new CultureInfo("ja-JP");

    driver.Down().Space().Enter();

    Check.SequenceEqual(["りんご"], Prompt.MultiSelect("果物", ["りんご", "みかん"]));
    Check.Contains("<スペース> で選択", driver.Output);
});

runner.Run("Escape throws PromptCanceledException", driver =>
{
    driver.Key(ConsoleKey.Escape);

    Check.Throws<PromptCanceledException>(() => Prompt.Input<string>("Value"));
});

runner.Run("A canceled token stops Bind before any prompt is shown", driver =>
{
    using var cts = new CancellationTokenSource();

    cts.Cancel();

    Check.Throws<OperationCanceledException>(() => Prompt.Bind<SignupModel>(cts.Token));
    Check.Equal("", driver.Output);
});

return runner.Complete();

void InputConverts<T>(string typeName, string text, T expected)
{
    runner.Run($"Input<{typeName}> converts \"{text}\"", driver =>
    {
        driver.Type(text).Enter();

        Check.Equal(expected, Prompt.Input<T>("Value"));
    });
}

void EditsDefaultValue<T>(string typeName, T defaultValue)
{
    runner.Run($"Input<{typeName}> round-trips the default value edited with Tab", driver =>
    {
        driver.Tab().Enter();

        Check.Equal(defaultValue, Prompt.Input<T>(options =>
        {
            options.Message = "Value";
            options.DefaultValue = defaultValue;
        }));
    });
}

void SelectsEnum<TEnum>(string metadataSource, TEnum expected) where TEnum : struct, Enum
{
    runner.Run($"Select<{typeof(TEnum).Name}> lists items from the {metadataSource}", driver =>
    {
        driver.Down(3).Enter();

        Check.Equal(expected, Prompt.Select<TEnum>("Priority"));

        // Display.Order first, then the rest; the alias with the ordinally first name represents its value.
        Check.InOrder(driver.Output, "High priority", "Medium priority", "Default priority");
        Check.DoesNotContain("Low", driver.Output);
    });
}

void MultiSelectsAllEnumItems<TEnum>(string metadataSource, TEnum[] expected) where TEnum : struct, Enum
{
    runner.Run($"MultiSelect<{typeof(TEnum).Name}> selects all items from the {metadataSource}", driver =>
    {
        driver.Key(ConsoleKey.A, '\x01', ConsoleModifiers.Control).Enter();

        Check.SequenceEqual(expected, Prompt.MultiSelect<TEnum>("Priorities"));
    });
}
