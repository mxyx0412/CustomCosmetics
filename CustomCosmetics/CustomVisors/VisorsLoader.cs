using CustomCosmetics.Core;
using System.Collections.Generic;
using System.Text.Json;

namespace CustomCosmetics.CustomVisors;

public class VisorsLoader : CosmeticsLoader
{
    protected override string Kind => "visors";
    protected override string ConfigFile => "CustomVisors.json";
    protected override string ResDir => "visors";
    protected override string LocalDir => CosmeticsManager.CustomVisorsDir;
    protected override string ConfigFor(RepositorySource src) => src.VisorsFile ?? ConfigFile;
    protected override string ResDirFor(RepositorySource src) => src.VisorsDir ?? ResDir;

    protected override void OnConfig(string json)
    {
        var r = JsonSerializer.Deserialize<VisorsConfigFile>(json, new JsonSerializerOptions
        {
            AllowTrailingCommas = true,
            PropertyNameCaseInsensitive = true
        });
        if (r == null || r.Visors == null) return;

        RegisterPackages(r.Packages, CustomVisorManager.PackageDisplayNames, CustomVisorManager.PackagePriorities);

        foreach (var v in r.Visors)
        {
            v.Resource = PrefixResource(v.Resource);
            v.FlipResource = PrefixResource(v.FlipResource);
        }
        CustomVisorManager.UnregisteredVisors.AddRange(r.Visors);
        Message($"Loaded {r.Visors.Count} visors ({MissingFiles().Count} resource files)");
    }

    protected override List<string> MissingFiles()
    {
        var list = new List<string>();
        foreach (var v in CustomVisorManager.UnregisteredVisors)
        {
            if (v.Resource != null && NeedDownload(v.Resource, v.ResHashA, LocalDir)) list.Add(v.Resource);
            if (v.FlipResource != null && NeedDownload(v.FlipResource, v.ResHashF, LocalDir)) list.Add(v.FlipResource);
        }
        return list;
    }
}
