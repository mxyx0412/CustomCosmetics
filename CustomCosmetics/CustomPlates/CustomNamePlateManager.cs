using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CustomCosmetics.CustomPlates;

public static class CustomNamePlateManager
{
    internal static List<CustomNamePlateConfig> UnregisteredPlates = new();
    // cache key: ProductId (cmp_*)
    internal static readonly Dictionary<string, NamePlateViewData> ViewDataCache = new();
    internal static readonly Dictionary<string, string> AuthorCache = new();
    internal static readonly Dictionary<string, string> PackageCache = new();
    internal static readonly Dictionary<string, string> PackageDisplayNames = new();
    internal static readonly Dictionary<string, int> PackagePriorities = new();

    internal static NamePlateData CreatePlateBehaviour(CustomNamePlateConfig config)
    {
        var spr = CreatePlateSprite(config.Resource);
        if (spr == null)
            throw new FileNotFoundException("Nameplate file not downloaded yet");

        var pid = $"cmp_{config.Name.Replace(' ', '_')}";

        // ViewDataRef unset: renders via CosmeticsCache.GetNameplate shortcut
        var viewData = ViewDataCache[pid] = ScriptableObject.CreateInstance<NamePlateViewData>();
        viewData.Image = spr;

        var plate = ScriptableObject.CreateInstance<NamePlateData>();
        plate.name = config.Name;
        plate.ProductId = pid;
        plate.displayOrder = 0;
        plate.Free = true;
        plate.ChipOffset = new Vector2(0f, 0.2f);
        AuthorCache[plate.name] = config.Author ?? "Unknown";
        PackageCache[plate.name] = config.Package ?? "Misc.";
        return plate;
    }

    private static Sprite CreatePlateSprite(string path)
    {
        var texture = Core.CosmeticsLoader.LoadTex(
            Path.Combine(CosmeticsManager.CustomPlatesDir, path));
        if (texture == null) return null;
        // base 275px -> 2.75 units
        return Core.CosmeticsLoader.MakeSprite(texture, new Vector2(0.5f, 0.5f), texture.width / 2.75f);
    }
}
