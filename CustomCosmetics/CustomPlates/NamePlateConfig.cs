using CustomCosmetics.Core;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CustomCosmetics.CustomPlates;

public class NamePlatesConfigFile
{
    [JsonPropertyName("packages")] public List<PackageConfig> Packages { get; set; }
    [JsonPropertyName("nameplates")] public List<CustomNamePlateConfig> NamePlates { get; set; }
}

public class CustomNamePlateConfig
{
    [JsonPropertyName("author")] public string Author { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; }
    [JsonPropertyName("package")] public string Package { get; set; }
    [JsonPropertyName("resource")] public string Resource { get; set; }
    [JsonPropertyName("reshasha")] public string ResHashA { get; set; }
}
