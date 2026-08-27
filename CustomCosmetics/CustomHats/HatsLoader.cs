using CustomCosmetics.Core;
using System.Collections.Generic;
using System.Text.Json;

namespace CustomCosmetics.CustomHats;

public class HatsLoader : CosmeticsLoader
{
    protected override string Kind => "hats";
    protected override string ConfigFile => "CustomHats.json";
    protected override string ResDir => "hats";
    protected override string LocalDir => CosmeticsManager.CustomHatsDir;
    protected override string ConfigFor(RepositorySource src) => src.HatsFile ?? ConfigFile;
    protected override string ResDirFor(RepositorySource src) => src.HatsDir ?? ResDir;

    protected override void OnConfig(string json)
    {
        var r = JsonSerializer.Deserialize<HatsConfigFile>(json, new JsonSerializerOptions
        {
            AllowTrailingCommas = true,
            PropertyNameCaseInsensitive = true
        });
        if (r == null || r.Hats == null) return;

        RegisterPackages(r.Packages, CustomHatManager.PackageDisplayNames, CustomHatManager.PackagePriorities);
        Sanitize(r);
        CustomHatManager.UnregisteredHats.AddRange(r.Hats);
        Message($"Loaded {r.Hats.Count} hats ({MissingFiles().Count} resource files)");
    }

    protected override List<string> MissingFiles()
    {
        return CustomHatManager.GenerateDownloadList(CustomHatManager.UnregisteredHats);
    }

    private void Sanitize(HatsConfigFile r)
    {
        foreach (var h in r.Hats)
        {
            h.Resource = PrefixResource(h.Resource);
            h.BackResource = PrefixResource(h.BackResource);
            h.ClimbResource = PrefixResource(h.ClimbResource);
            h.FlipResource = PrefixResource(h.FlipResource);
            h.BackFlipResource = PrefixResource(h.BackFlipResource);
        }
    }
}
