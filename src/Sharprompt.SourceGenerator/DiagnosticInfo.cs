using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Sharprompt.SourceGenerator;

/// <summary>
/// An equatable description of a diagnostic. <see cref="Diagnostic"/> and <see cref="Location"/> hold
/// on to syntax trees and would defeat incremental caching, so only their plain values are kept.
/// </summary>
internal sealed record DiagnosticInfo(DiagnosticDescriptor Descriptor, LocationInfo? Location, EquatableArray<string> MessageArgs)
{
    public static DiagnosticInfo Create(DiagnosticDescriptor descriptor, Location? location, params string[] messageArgs)
    {
        return new DiagnosticInfo(descriptor, LocationInfo.From(location), new EquatableArray<string>(messageArgs));
    }

    public Diagnostic ToDiagnostic()
    {
        return Diagnostic.Create(Descriptor, Location?.ToLocation() ?? Microsoft.CodeAnalysis.Location.None, MessageArgs.Cast<object>().ToArray());
    }
}

internal sealed record LocationInfo(string FilePath, TextSpan TextSpan, LinePositionSpan LineSpan)
{
    public static LocationInfo? From(Location? location)
    {
        if (location is null || !location.IsInSource)
        {
            return null;
        }

        return new LocationInfo(location.SourceTree!.FilePath, location.SourceSpan, location.GetLineSpan().Span);
    }

    public Location ToLocation() => Location.Create(FilePath, TextSpan, LineSpan);
}
