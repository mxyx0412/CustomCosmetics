using AmongUs.Data;

namespace CustomCosmetics.CustomPlates.Patches;

[HarmonyPatch(typeof(PlayerVoteArea))]
internal static class PlayerVoteAreaPatches
{
    [HarmonyPatch(nameof(PlayerVoteArea.PreviewNameplate))]
    [HarmonyPrefix]
    private static bool PreviewNameplatePrefix(PlayerVoteArea __instance, string plateID)
    {
        // skip vanilla GUID async load; set NameText/LevelNumberText manually (prefix bypasses them)
        if (!plateID.StartsWith("cmp_")) return true;
        if (CustomNamePlateManager.ViewDataCache.TryGetValue(plateID, out var vd) && vd.Image != null)
            __instance.Background.sprite = vd.Image;
        __instance.PlayerIcon.gameObject.SetActive(false);
        __instance.NameText.text = DataManager.Player.Customization.Name;
        __instance.LevelNumberText.text = ProgressionManager.Instance.CurrentVisualLevel;
        return false;
    }
}