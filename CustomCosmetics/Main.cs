global using HarmonyLib;
global using static CustomCosmetics.Logger;
global using ISystem = Il2CppSystem.Collections.Generic;
global using Main = CustomCosmetics.CosmeticsManager;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using CustomCosmetics.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace CustomCosmetics;

[BepInAutoPlugin("com.mxyx.cosmetics")]
[BepInProcess("Among Us.exe")]
public partial class CosmeticsManager : BasePlugin
{
    private const string MOD_FOLDER = "CustomCosmetics";

    internal static string CosmeticDir => Path.Combine(Application.persistentDataPath, MOD_FOLDER);
    internal static string CustomHatsDir => Path.Combine(CosmeticDir, "CustomHats");
    internal static string CustomVisorsDir => Path.Combine(CosmeticDir, "CustomVisors");
    internal static string CustomPlatesDir => Path.Combine(CosmeticDir, "CustomPlates");

    public static Main Instance { get; set; }
    public Harmony Harmony { get; } = new(Id);

    internal static ConfigEntry<bool> EnableHats { get; set; }
    internal static ConfigEntry<bool> EnableVisors { get; set; }
    internal static ConfigEntry<bool> EnablePlates { get; set; }
    internal static ConfigEntry<string> Repositories { get; set; }

    internal static List<RepositorySource> Repos { get; private set; } = new();

    public override void Load()
    {
        SetLogSource(Log);
        Harmony.PatchAll();

        EnableHats = Config.Bind("Cosmetics", "EnableHats", true, "Enable custom hats loading");
        EnableVisors = Config.Bind("Cosmetics", "EnableVisors", false, "Enable custom visors loading");
        EnablePlates = Config.Bind("Cosmetics", "EnableNamePlates", false, "Enable custom name plates loading");
        Repositories = Config.Bind("Cosmetics", "Repositories",
            "https://raw.githubusercontent.com/TheOtherRolesAU/TheOtherHats/master|hat",
            "Repository URLs. Format: url|flags (flags: hat/visor/plate), separate with ;");

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
        var reposStr = Repositories.Value;
        if (string.IsNullOrWhiteSpace(reposStr)) return;

        var entries = reposStr.Split(';', StringSplitOptions.RemoveEmptyEntries);
        foreach (var entry in entries)
        {
            var parts = entry.Trim().Split('|', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) continue;

            var url = parts[0].Trim().TrimEnd('/').GithubUrl();
            if (string.IsNullOrWhiteSpace(url)) continue;

            var flags = parts.Length > 1 ? parts[1].ToLower() : "hat";
            var src = new RepositorySource
            {
                Url = url,
                Alias = PathFromUrl(url),
                Hats = flags.Contains("hat"),
                Visors = flags.Contains("visor"),
                Nameplates = flags.Contains("plate")
            };

            if (!src.Hats && !src.Visors && !src.Nameplates) src.Hats = true;

            Repos.Add(src);

            var t = new List<string>();
            if (src.Hats) t.Add("hats");
            if (src.Visors) t.Add("visors");
            if (src.Nameplates) t.Add("nameplates");
            Message($"Repository: {src.Url} [{string.Join(", ", t)}]");
        }
    }

    private static string PathFromUrl(string url)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        var hash = System.BitConverter.ToString(
            md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(url))).Replace("-", "").ToLowerInvariant();
        return hash[..8];
    }
}

[Serializable]
public class RepositorySource
{
    public string Url;
    public string Alias;
    public bool Hats;
    public bool Visors;
    public bool Nameplates;
    public string HatsFile;
    public string VisorsFile;
    public string PlatesFile;
    public string HatsDir;
    public string VisorsDir;
    public string PlatesDir;
}
