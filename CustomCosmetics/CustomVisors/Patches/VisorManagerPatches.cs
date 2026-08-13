using Cpp2IL.Core.Extensions;
using System;
using System.Linq;

namespace CustomCosmetics.CustomVisors.Patches;

[HarmonyPatch(typeof(HatManager))]
internal static class VisorManagerPatches
{
    private static bool _isLoaded;

    [HarmonyPatch(nameof(HatManager.GetVisorById))]
    [HarmonyPrefix]
    private static void GetVisorByIdPrefix(HatManager __instance)
    {
        if (_isLoaded) return;

        var allVisors = __instance.allVisors.ToList();
        var cache = CustomVisorManager.UnregisteredVisors.Clone();
        foreach (var visorConfig in cache)
        {
            try
            {
                allVisors.Add(CustomVisorManager.CreateVisorBehaviour(visorConfig));
                CustomVisorManager.UnregisteredVisors.Remove(visorConfig);
            }
            catch { }
        }
        __instance.allVisors = allVisors.ToArray();
        if (CustomVisorManager.UnregisteredVisors.Count == 0) _isLoaded = true;
    }

    [HarmonyPatch(typeof(CosmeticsCache), nameof(CosmeticsCache.GetVisor))]
    [HarmonyPrefix]
    private static bool GetVisorPrefix(string id, ref VisorViewData __result)
    {
        return !CustomVisorManager.ViewDataCache.TryGetValue(id, out __result);
    }
}
