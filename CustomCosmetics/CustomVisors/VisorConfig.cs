using CustomCosmetics.Core;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CustomCosmetics.CustomVisors;

public class VisorsConfigFile
{
    [JsonPropertyName("packages")] public List<PackageConfig> Packages { get; set; }
    [JsonPropertyName("visors")] public List<CustomVisorConfig> Visors { get; set; }
}

public class CustomVisorConfig
{
    [JsonPropertyName("author")] public string Author { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; }
    [JsonPropertyName("package")] public string Package { get; set; }
    [JsonPropertyName("resource")] public string Resource { get; set; }
    [JsonPropertyName("flipresource")] public string FlipResource { get; set; }

    [JsonPropertyName("behindHats")]
    [JsonConverter(typeof(LooseBoolConverter))]
    public bool BehindHats { get; set; }

    [JsonPropertyName("adaptive")]
    [JsonConverter(typeof(LooseBoolConverter))]
    public bool Adaptive { get; set; }

    // default off
    [JsonPropertyName("autoscale")]
    [JsonConverter(typeof(LooseBoolConverter))]
    public bool AutoScale { get; set; }

    [JsonPropertyName("reshasha")] public string ResHashA { get; set; }
    [JsonPropertyName("reshashf")] public string ResHashF { get; set; }
}

public class LooseBoolConverter : JsonConverter<bool>
{
    public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.True => true,
            JsonTokenType.False => false,
            JsonTokenType.String => reader.GetString()?.ToLowerInvariant() == "true",
            _ => throw new JsonException($"Cannot convert {reader.TokenType} to bool")
        };
    }

    public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
        => writer.WriteBooleanValue(value);
}
