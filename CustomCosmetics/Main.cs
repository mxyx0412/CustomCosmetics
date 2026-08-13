global using HarmonyLib;
global using static CustomCosmetics.Logger;
global using ISystem = Il2CppSystem.Collections.Generic;
global using Main = CustomCosmetics.CosmeticsManager;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using CustomCosmetics.Core;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace CustomCosmetics;

[BepInAutoPlugin("com.mxyx.cosmetics")]
[BepInProcess("Among Us.exe")]
public partial class CosmeticsManager : BasePlugin
{
    internal static string CosmeticDir = Path.Combine(Paths.GameRootPath, "Cosmetics");
    internal static string CustomHatsDir => Path.Combine(CosmeticDir, "CustomHats");
    internal static string CustomVisorsDir => Path.Combine(CosmeticDir, "CustomVisors");
    internal static string CustomPlatesDir => Path.Combine(CosmeticDir, "CustomPlates");

    public static Main Instance { get; set; }
    public Harmony Harmony { get; } = new(Id);

    internal static YamlConfigManager YamlConfig { get; private set; }
    internal static ConfigOption<bool> Unlocker { get; set; }
    internal static ConfigOption<bool> LocalOnly { get; set; }
    internal static ConfigOption<bool> EnableHats { get; set; }
    internal static ConfigOption<bool> EnableVisors { get; set; }
    internal static ConfigOption<bool> EnablePlates { get; set; }

    internal static List<RepositorySource> Repos { get; private set; } = new();

    public override void Load()
    {
        SetLogSource(Log);
        Harmony.PatchAll();

        // config keys: Cosmetics/config.yml
        YamlConfig = new YamlConfigManager("Cosmetics/config.yml");
        Unlocker = YamlConfig.CreateOption("cosmetics.unlocker", false);
        LocalOnly = YamlConfig.CreateOption("cosmetics.local", false);
        EnableHats = YamlConfig.CreateOption("hats.enabled", true);
        EnableVisors = YamlConfig.CreateOption("visors.enabled", false);
        EnablePlates = YamlConfig.CreateOption("nameplates.enabled", false);
        YamlConfig.Load();
        LoadRepos();

        Instance = this;

        foreach (var dir in new[] { CustomHatsDir, CustomVisorsDir, CustomPlatesDir })
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        var hats = AddComponent<CustomHats.HatsLoader>();
        var visors = AddComponent<CustomVisors.VisorsLoader>();
        var plates = AddComponent<CustomPlates.NamePlatesLoader>();

        if (EnableHats.Value) hats.Fetch(Repos.Where(r => r.Hats).ToList());
        if (EnableVisors.Value) visors.Fetch(Repos.Where(r => r.Visors).ToList());
        if (EnablePlates.Value) plates.Fetch(Repos.Where(r => r.Nameplates).ToList());

        Message("CosmeticsManager loaded!");
    }

    internal static void LoadRepos()
    {
        Repos.Clear();
        var entries = YamlConfig.ReadNode<List<RepositorySource>>("repositories");
        if (entries == null || entries.Count == 0)
        {
            // fallback: write default repo on empty config
            YamlConfig.WriteNode("repositories", new List<RepositorySource>
            {
                new() { Url = "https://raw.githubusercontent.com/TheOtherRolesAU/TheOtherHats/master", Hats = true }
            });
            entries = YamlConfig.ReadNode<List<RepositorySource>>("repositories");
        }
        if (entries == null) return;

        foreach (var entry in entries)
        {
            if (string.IsNullOrWhiteSpace(entry.Url)) continue;
            entry.Url = entry.Url.Trim().TrimEnd('/').GithubUrl();
            entry.Alias ??= PathFromUrl(entry.Url);

            Repos.Add(entry);

            var t = new List<string>();
            if (entry.Hats) t.Add("hats");
            if (entry.Visors) t.Add("visors");
            if (entry.Nameplates) t.Add("nameplates");
            Message($"Repository: {entry.Url} [{string.Join(", ", t)}]");
        }
    }

    private static string PathFromUrl(string url)
    {
        try
        {
            var u = new System.Uri(url);
            var segs = u.AbsolutePath.Trim('/').Split('/').Where(s => s.Length > 0
                && s != "master" && s != "main" && s != "HEAD"
                && !(s.StartsWith("v") && s.Length > 1 && char.IsDigit(s[1]))
                && !s.All(c => char.IsDigit(c) || c == '.')).ToArray();
            if (segs.Length > 0) return Regex.Replace(string.Join("_", segs), @"[^a-zA-Z0-9_\-]", "_");
        }
        catch { }
        using var md5 = System.Security.Cryptography.MD5.Create();
        var hash = System.BitConverter.ToString(
            md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(url))).Replace("-", "").ToLowerInvariant();
        return hash[..8];
    }
}

public class RepositorySource
{
    public string Url { get; set; }
    public string Alias { get; set; }
    public bool Hats { get; set; }
    public bool Visors { get; set; }
    public bool Nameplates { get; set; }
    public string HatsFile { get; set; }
    public string VisorsFile { get; set; }
    public string PlatesFile { get; set; }
    public string HatsDir { get; set; }
    public string VisorsDir { get; set; }
    public string PlatesDir { get; set; }
}
