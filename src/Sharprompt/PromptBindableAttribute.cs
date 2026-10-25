using System;

namespace Sharprompt;

/// <summary>
/// Generates a binder for a class or record class at compile time. Bound properties need accessible get/set accessors; init-only properties are skipped.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class PromptBindableAttribute : Attribute;
