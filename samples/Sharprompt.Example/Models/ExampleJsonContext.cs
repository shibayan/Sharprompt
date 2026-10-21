using System.Text.Json.Serialization;

namespace Sharprompt.Example.Models;

// Source-generated serialization, which works with Native AOT; reflection-based JsonSerializer calls do not.
[JsonSerializable(typeof(MyFormModel))]
internal partial class ExampleJsonContext : JsonSerializerContext;
