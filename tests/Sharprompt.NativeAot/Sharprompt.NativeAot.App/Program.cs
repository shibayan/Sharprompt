using System;
using System.Globalization;
using System.Threading;

using Sharprompt;
using Sharprompt.NativeAot;
using Sharprompt.NativeAot.External;

// Fix the culture so the assertions do not depend on the machine running them.
CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

Prompt.ThrowExceptionOnCancel = true;

var runner = new ScenarioRunner();

// Input: typed text is converted through TypeDescriptor converters with the invariant culture.
// These are the types the documentation lists as supported.
InputConverts("string", "Sharprompt", "Sharprompt");
InputConverts("bool", "true", true);
InputConverts("char", "x", 'x');
InputConverts("byte", "255", (byte)255);
InputConverts("sbyte", "-128", (sbyte)-128);
InputConverts("short", "-3", (short)-3);
InputConverts("ushort", "3", (ushort)3);
InputConverts("int", "42", 42);
InputConverts("uint", "3", 3u);
InputConverts("long?", "9000000000", (long?)9_000_000_000);
InputConverts("ulong", "18446744073709551615", ulong.MaxValue);
InputConverts("Int128", "170141183460469231731687303715884105727", Int128.MaxValue);
InputConverts("Half", "1.5", (Half)1.5);
InputConverts("float", "1.25", 1.25f);
InputConverts("double", "1.5", 1.5);
InputConverts("decimal", "0.1", 0.1m);
InputConverts("Guid", "8f2c1b9e-3d4a-4e5f-9a6b-7c8d9e0f1a2b", new Guid("8f2c1b9e-3d4a-4e5f-9a6b-7c8d9e0f1a2b"));
InputConverts("DateTime", "2026-10-21T09:30:00", new DateTime(2026, 10, 21, 9, 30, 0));
InputConverts("DateTimeOffset", "2026-10-21T09:30:00+09:00", new DateTimeOffset(2026, 10, 21, 9, 30, 0, TimeSpan.FromHours(9)));
InputConverts("DateOnly", "2026-10-21", new DateOnly(2026, 10, 21));
InputConverts("TimeOnly", "09:30", new TimeOnly(9, 30));
InputConverts("TimeSpan", "01:02:03", new TimeSpan(1, 2, 3));
InputConverts("Uri", "https://example.com/", new Uri("https://example.com/"));
InputConverts("Version", "1.2.3", new Version(1, 2, 3));
InputConverts("LocalPriority", "High", LocalPriority.High);
InputConverts("ExternalPriority", "Medium", ExternalPriority.Medium);
InputConverts("LocalPriority", "99", (LocalPriority)99);

// Custom types rely on the converter named by [TypeConverter], in the app and in a trimmable library.
InputConverts("Temperature", "21.5C", new Temperature(21.5));
InputConverts("ExternalTemperature", "3C", new ExternalTemperature(3));

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
EditsDefaultValue("float", 0.1f);
EditsDefaultValue("double", 0.1 + 0.2);
EditsDefaultValue("decimal", 12.34m);
EditsDefaultValue("DateTime", new DateTime(2026, 10, 21, 9, 30, 0));
EditsDefaultValue("DateTimeOffset", new DateTimeOffset(2026, 10, 21, 9, 30, 0, TimeSpan.FromHours(9)));
EditsDefaultValue("DateOnly", new DateOnly(2026, 10, 21));
EditsDefaultValue("TimeSpan", new TimeSpan(1, 2, 3));
EditsDefaultValue("TimeOnly", TimeOnly.MinValue); // midnight, which TimeOnlyConverter formats as ""
EditsDefaultValue("DateTime", DateTime.MinValue); // also formatted as "" by its converter
EditsDefaultValue("LocalPriority", LocalPriority.Medium);
EditsDefaultValue("ExternalPriority?", (ExternalPriority?)(ExternalPriority)99);
EditsDefaultValue("Temperature", new Temperature(21.5));
EditsDefaultValue("ExternalTemperature", new ExternalTemperature(3));

runner.Run("Input<double> shows the default value and answer in the format it parses", driver =>
{
    var culture = CultureInfo.CurrentCulture;

    // de-DE formats 1.5 as "1,5", which input parsing (invariant culture) would not accept.
    CultureInfo.CurrentCulture = new CultureInfo("de-DE");

    try
    {
        driver.Enter();

        Check.Equal(1.5, Prompt.Input<double>("Value", defaultValue: 1.5));
        Check.Contains("(1.5)", driver.Output);
        Check.Contains("Value: 1.5", driver.Output);
        Check.DoesNotContain("1,5", driver.Output);
    }
    finally
    {
        CultureInfo.CurrentCulture = culture;
    }
});

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
    // The input template ("?") with the whole password typed; the finished answer ("✔") is masked as well.
    Check.Contains("? Password: *******", driver.Output);
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

// Display names from DisplayAttribute.ResourceType are read each time items are shown, on both metadata paths.
SelectsLocalizedEnum("generated metadata", LocalTimeOfDay.Evening);
SelectsLocalizedEnum("reflection fallback", ExternalTimeOfDay.Evening);

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
    Check.Contains("? Secret: *******", driver.Output);
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
    runner.Run($"Input<{typeName}> round-trips the default value {defaultValue} edited with Tab", driver =>
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

void SelectsLocalizedEnum<TEnum>(string metadataSource, TEnum expected) where TEnum : struct, Enum
{
    runner.Run($"Select<{typeof(TEnum).Name}> reads display names from resources through the {metadataSource}", driver =>
    {
        driver.Down(2).Enter().Down(2).Enter();

        Check.Equal(expected, Prompt.Select<TEnum>("Time"));
        Check.InOrder(driver.Output, "Morning", "Evening");

        var culture = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentUICulture = new CultureInfo("ja-JP");

        try
        {
            Check.Equal(expected, Prompt.Select<TEnum>("Time"));
            Check.InOrder(driver.Output, "朝", "夜");
        }
        finally
        {
            CultureInfo.CurrentUICulture = culture;
        }
    });
}
