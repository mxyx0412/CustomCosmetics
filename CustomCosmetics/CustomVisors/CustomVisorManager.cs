using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace CustomCosmetics.CustomVisors;

public static class CustomVisorManager
{
    internal static List<CustomVisorConfig> UnregisteredVisors = new();
    internal static readonly Dictionary<string, VisorViewData> ViewDataCache = new();
    // cache key: visor.ProductId (cmv_*)
    internal static readonly Dictionary<string, bool> BehindHatsCache = new();
    internal static readonly Dictionary<string, string> AuthorCache = new();
    internal static readonly Dictionary<string, string> PackageCache = new();
    internal static readonly Dictionary<string, string> PackageDisplayNames = new();
    internal static readonly Dictionary<string, int> PackagePriorities = new();

    internal static VisorData CreateVisorBehaviour(CustomVisorConfig config)
    {
        var viewData = ViewDataCache[config.Name] = ScriptableObject.CreateInstance<VisorViewData>();
        var spr = CreateVisorSprite(config.Resource, config.AutoScale);
        if (spr == null)
            throw new FileNotFoundException("Visor file not downloaded yet");
        viewData.IdleFrame = spr;

        if (config.FlipResource != null)
            viewData.LeftIdleFrame = CreateVisorSprite(config.FlipResource, config.AutoScale);
        viewData.MatchPlayerColor = config.Adaptive;

        var visor = ScriptableObject.CreateInstance<VisorData>();
        visor.name = config.Name;
        visor.displayOrder = 0;
        visor.ProductId = $"cmv_{config.Name.Replace(' ', '_')}";
        visor.BundleId = visor.ProductId;
        visor.Free = true;
        visor.ChipOffset = new Vector2(0f, 0.2f);
        visor.SpritePreview = spr;
        visor.ViewDataRef = new AssetReference(viewData.Pointer);
        visor.CreateAddressableAsset();
        BehindHatsCache[visor.ProductId] = config.BehindHats;
        AuthorCache[visor.name] = config.Author ?? "Unknown";
        PackageCache[visor.name] = config.Package ?? "Misc.";
        return visor;
    }

    private static Sprite CreateVisorSprite(string path, bool autoScale)
    {
        var texture = Core.CosmeticsLoader.LoadTex(
            Path.Combine(CosmeticsManager.CustomVisorsDir, path));
        if (texture == null) return null;
        // autoScale: 300px base ppu; off: raw pixels (ppu 100)
        return Core.CosmeticsLoader.MakeSprite(texture,
            new Vector2(0.5f, 0.5f),
            autoScale ? texture.width * 0.375f : 100f);
    }
}
