using System;
using System.Collections.Generic;

namespace Sharprompt.NativeAot;

internal sealed class ScenarioRunner
{
    private readonly List<string> _failures = [];
    private int _count;

    public void Run(string name, Action<ScriptedConsoleDriver> scenario)
    {
        var driver = new ScriptedConsoleDriver();
        var culture = Prompt.Culture;

        Prompt.ConsoleDriverFactory = () => driver;

        _count++;

        try
        {
            scenario(driver);

            if (driver.RemainingKeys > 0)
            {
                throw new VerificationException($"{driver.RemainingKeys} scripted key(s) were not consumed.");
            }

            Console.WriteLine($"PASS {name}");
        }
        catch (Exception ex)
        {
            _failures.Add(name);

            Console.WriteLine($"FAIL {name}");
            Console.WriteLine(Indent(ex.ToString()));
            Console.WriteLine("  Rendered output:");
            Console.WriteLine(Indent(driver.Output));
        }
        finally
        {
            Prompt.Culture = culture;
        }
    }

    public int Complete()
    {
        Console.WriteLine();
        Console.WriteLine($"{_count - _failures.Count} passed, {_failures.Count} failed.");

        foreach (var failure in _failures)
        {
            Console.WriteLine($"  FAIL {failure}");
        }

        return _failures.Count == 0 ? 0 : 1;
    }

    private static string Indent(string text) => "    " + text.ReplaceLineEndings(Environment.NewLine + "    ");
}
