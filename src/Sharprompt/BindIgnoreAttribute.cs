using System;

namespace Sharprompt;

/// <summary>
/// Excludes a property from source-generated model binding.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class BindIgnoreAttribute : Attribute;
