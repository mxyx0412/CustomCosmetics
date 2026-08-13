using System.Text.Json.Serialization;

namespace CustomCosmetics.Core;

public class PackageConfig
{
    [JsonPropertyName("Package")] public string Package { get; set; }
    [JsonPropertyName("DisplayName")] public string DisplayName { get; set; }
    [JsonPropertyName("Priority")] public int Priority { get; set; }
}
