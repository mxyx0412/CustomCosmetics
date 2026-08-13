using Cpp2IL.Core.Extensions;
using System;
using System.Linq;

namespace CustomCosmetics.CustomHats.Patches;

[HarmonyPatch(typeof(HatManager))]
internal static class HatManagerPatches
{
    private static bool _isRunning;
    private static bool _isLoaded;

    [HarmonyPatch(nameof(HatManager.GetHatById))]
    [HarmonyPrefix]
    private static void GetHatByIdPrefix(HatManager __instance)
    {
        // inject once; priority: Innersloth=100, custom=config or 50
        if (_isRunning || _isLoaded) return;
        _isRunning = true;
        var allHats = __instance.allHats.ToList();
        var cache = CustomHatManager.UnregisteredHats.Clone();
        foreach (var hat in cache)
        {
            try
            {
                allHats.Add(CustomHatManager.CreateHatBehaviour(hat));
                CustomHatManager.UnregisteredHats.Remove(hat);
            }
            catch { }
        }
        if (CustomHatManager.UnregisteredHats.Count == 0) _isLoaded = true;
        cache.Clear();
        __instance.allHats = allHats.OrderBy(PriorityFor).ToArray();
    }

    private static int PriorityFor(HatData h)
    {
        var ext = h.GetHatExtension();
        var pkg = ext?.Package ?? "Innersloth";
        if (pkg == CustomHatManager.InnerslothPackageName) return 100;
        return CustomHatManager.PackagePriorities.TryGetValue(pkg, out var pr) ? pr : 50;
    }

    [HarmonyPatch(nameof(HatManager.GetHatById))]
    [HarmonyPostfix]
    private static void GetHatByIdPostfix()
    {
        _isRunning = false;
    }

    [HarmonyPatch(typeof(CosmeticsCache), nameof(CosmeticsCache.GetHat))]
    [HarmonyPrefix]
    private static bool GetHatPrefix(string id, ref HatViewData __result)
    {
        return !CustomHatManager.ViewDataCache.TryGetValue(id, out __result);
    }
}
