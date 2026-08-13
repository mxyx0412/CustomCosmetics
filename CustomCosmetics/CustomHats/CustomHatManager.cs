using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace CustomCosmetics.CustomHats;

public static class CustomHatManager
{
    public const string InnerslothPackageName = "Innersloth Hats";

    internal static List<CustomHatConfig> UnregisteredHats = new();
    // cache key: hat.name
    internal static readonly Dictionary<string, HatViewData> ViewDataCache = new();
    internal static readonly Dictionary<string, HatExtension> ExtensionCache = new();
    internal static readonly Dictionary<string, string> AuthorCache = new();
    internal static readonly Dictionary<string, string> PackageDisplayNames = new();
    internal static readonly Dictionary<string, int> PackagePriorities = new();

    internal static HatExtension TestExtension { get; private set; }

    internal static bool TryGetCached(this HatParent hatParent, out HatViewData asset)
    {
        if (hatParent && hatParent.Hat) return hatParent.Hat.TryGetCached(out asset);
        asset = null;
        return false;
    }

    internal static bool TryGetCached(this HatData hat, out HatViewData asset)
    {
        return ViewDataCache.TryGetValue(hat.name, out asset);
    }

    internal static bool IsCached(this HatData hat) => ViewDataCache.ContainsKey(hat.name);
    internal static bool IsCached(this HatParent hatParent) => hatParent.Hat != null && hatParent.Hat.IsCached();

    internal static HatData CreateHatBehaviour(CustomHatConfig ch, bool testOnly = false)
    {
        var viewData = ViewDataCache[ch.Name] = ScriptableObject.CreateInstance<HatViewData>();
        var hat = ScriptableObject.CreateInstance<HatData>();

        viewData.MainImage = CreateHatSprite(ch.Resource, ch.AutoScale);
        if (viewData.MainImage == null)
            throw new FileNotFoundException("File not downloaded yet");
        viewData.FloorImage = viewData.MainImage;

        if (ch.BackResource != null)
        {
            viewData.BackImage = CreateHatSprite(ch.BackResource, ch.AutoScale);
            ch.Behind = true;
        }
        if (ch.ClimbResource != null)
        {
            viewData.ClimbImage = CreateHatSprite(ch.ClimbResource, ch.AutoScale);
            viewData.LeftClimbImage = viewData.ClimbImage;
        }

        hat.name = ch.Name;
        hat.displayOrder = 0;
        hat.ProductId = "cmh_" + ch.Name.Replace(' ', '_');
        hat.InFront = !ch.Behind;
        hat.NoBounce = !ch.Bounce;
        hat.ChipOffset = new Vector2(0f, 0.2f);
        hat.Free = true;
        viewData.MatchPlayerColor = ch.Adaptive;

        var extend = new HatExtension
        {
            Author = ch.Author ?? "Unknown",
            Package = ch.Package ?? "Misc.",
            Condition = ch.Condition ?? "none",
            Adaptive = ch.Adaptive
        };
        AuthorCache[hat.name] = extend.Author;

        if (ch.FlipResource != null)
            extend.FlipImage = CreateHatSprite(ch.FlipResource, ch.AutoScale);
        if (ch.BackFlipResource != null)
            extend.BackFlipImage = CreateHatSprite(ch.BackFlipResource, ch.AutoScale);

        if (testOnly)
        {
            TestExtension = extend;
            TestExtension.Condition = hat.name;
        }
        else
        {
            ExtensionCache[hat.name] = extend;
        }

        hat.ViewDataRef = new AssetReference(ViewDataCache[hat.name].Pointer);
        hat.CreateAddressableAsset();
        return hat;
    }

    // tutorial test hat: loads PNGs from Cosmetics/CustomHats/Test
    private static Sprite CreateHatSprite(string path, bool autoScale = true)
    {
        var texture = Core.CosmeticsLoader.LoadTex(
            Path.Combine(CosmeticsManager.CustomHatsDir, path))
            ?? Helpers.LoadTextureFromResources(path);
        if (texture == null) return null;
        // default: 300px base ppu; off: raw pixels (ppu 100)
        return Core.CosmeticsLoader.MakeSprite(texture,
            new Vector2(0.53f, 0.575f),
            autoScale ? texture.width * 0.375f : 100f);
    }

    public static List<CustomHatConfig> CreateHatDetailsFromFileNames(string[] fileNames, bool fromDisk = false)
    {
        var fronts = new Dictionary<string, CustomHatConfig>();
        var backs = new Dictionary<string, string>();
        var flips = new Dictionary<string, string>();
        var backFlips = new Dictionary<string, string>();
        var climbs = new Dictionary<string, string>();

        foreach (var fileName in fileNames)
        {
            var index = fileName.LastIndexOf("\\", StringComparison.InvariantCulture) + 1;
            var s = fromDisk ? fileName[index..].Split('.')[0] : fileName.Split('.')[3];
            var p = s.Split('_');
            var options = new HashSet<string>(p);
            if (options.Contains("back") && options.Contains("flip"))
                backFlips[p[0]] = fileName;
            else if (options.Contains("climb"))
                climbs[p[0]] = fileName;
            else if (options.Contains("back"))
                backs[p[0]] = fileName;
            else if (options.Contains("flip"))
                flips[p[0]] = fileName;
            else
                fronts[p[0]] = new CustomHatConfig
                {
                    Resource = fileName,
                    Name = p[0].Replace('-', ' '),
                    Bounce = options.Contains("bounce"),
                    Adaptive = options.Contains("adaptive"),
                    Behind = options.Contains("behind")
                };
        }

        var hats = new List<CustomHatConfig>();
        foreach (var frontKvP in fronts)
        {
            var k = frontKvP.Key;
            var hat = frontKvP.Value;
            backs.TryGetValue(k, out var backResource);
            climbs.TryGetValue(k, out var climbResource);
            flips.TryGetValue(k, out var flipResource);
            backFlips.TryGetValue(k, out var backFlipResource);
            if (backResource != null) hat.BackResource = backResource;
            if (climbResource != null) hat.ClimbResource = climbResource;
            if (flipResource != null) hat.FlipResource = flipResource;
            if (backFlipResource != null) hat.BackFlipResource = backFlipResource;
            if (hat.BackResource != null) hat.Behind = true;
            hats.Add(hat);
        }
        return hats;
    }

    internal static List<string> GenerateDownloadList(List<CustomHatConfig> hats)
    {
        var toDownload = new List<string>();
        foreach (var hat in hats)
        {
            var files = new List<(string, string)>
            {
                (hat.Resource, hat.ResHashA),
                (hat.BackResource, hat.ResHashB),
                (hat.ClimbResource, hat.ResHashC),
                (hat.FlipResource, hat.ResHashF),
                (hat.BackFlipResource, hat.ResHashBf)
            };
            foreach (var (fileName, fileHash) in files)
            {
                if (fileName != null && Core.CosmeticsLoader.NeedDownload(fileName, fileHash, CosmeticsManager.CustomHatsDir))
                    toDownload.Add(fileName);
            }
        }
        return toDownload;
    }
}
