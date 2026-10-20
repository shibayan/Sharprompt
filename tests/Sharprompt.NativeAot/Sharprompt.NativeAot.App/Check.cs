using System;
using System.Collections.Generic;
using System.Linq;

namespace Sharprompt.NativeAot;

internal sealed class VerificationException(string message) : Exception(message);

// Minimal assertions: a test framework would bring its own reflection and trimming concerns into the native binary.
internal static class Check
{
    public static void Equal<T>(T expected, T actual)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
        {
            throw new VerificationException($"Expected <{expected}> but was <{actual}>.");
        }
    }

    public static void SequenceEqual<T>(IEnumerable<T> expected, IEnumerable<T>? actual)
    {
        var expectedItems = expected.ToArray();
        var actualItems = actual?.ToArray();

        if (actualItems is null || !expectedItems.SequenceEqual(actualItems))
        {
            throw new VerificationException($"Expected [{string.Join(", ", expectedItems)}] but was [{(actualItems is null ? "null" : string.Join(", ", actualItems))}].");
        }
    }

    public static void Contains(string expected, string output)
    {
        if (!output.Contains(expected, StringComparison.Ordinal))
        {
            throw new VerificationException($"Expected the output to contain \"{expected}\".");
        }
    }

    public static void DoesNotContain(string unexpected, string output)
    {
        if (output.Contains(unexpected, StringComparison.Ordinal))
        {
            throw new VerificationException($"Expected the output not to contain \"{unexpected}\".");
        }
    }

    // Checks the order in which items are first rendered.
    public static void InOrder(string output, params string[] texts)
    {
        var previous = -1;

        foreach (var text in texts)
        {
            var index = output.IndexOf(text, StringComparison.Ordinal);

            if (index < 0)
            {
                throw new VerificationException($"Expected the output to contain \"{text}\".");
            }

            if (index < previous)
            {
                throw new VerificationException($"Expected the items in the order [{string.Join(", ", texts)}].");
            }

            previous = index;
        }
    }

    public static TException Throws<TException>(Action action) where TException : Exception
    {
        try
        {
            action();
        }
        catch (TException ex)
        {
            return ex;
        }

        throw new VerificationException($"Expected {typeof(TException).Name} to be thrown.");
    }
}
