using System.Collections.Generic;
using UnityEngine;

namespace CustomCosmetics.CustomVisors.Patches;

[HarmonyPatch(typeof(VisorLayer))]
internal static class VisorLayerPatches
{
    private static readonly Dictionary<string, VisorViewData> _cache = new();

    private static VisorViewData GetCache(string id)
    {
        if (!_cache.ContainsKey(id))
            _cache[id] = CustomVisorManager.ViewDataCache.TryGetValue(id, out var vd) ? vd : null;
        return _cache[id];
    }

    [HarmonyPatch(nameof(VisorLayer.UpdateMaterial))]
    [HarmonyPrefix]
    private static bool UpdateMaterialPrefix(VisorLayer __instance)
    {
        if (__instance.visorData == null || !__instance.visorData.ProductId.StartsWith("cmv_"))
            return true;

        var asset = GetCache(__instance.visorData.name);
        if (asset == null) return true;

        if (asset.MatchPlayerColor)
            __instance.Image.sharedMaterial = DestroyableSingleton<HatManager>.Instance.PlayerMaterial;
        else
            __instance.Image.sharedMaterial = DestroyableSingleton<HatManager>.Instance.DefaultShader;

        PlayerMaterial.SetColors(__instance.matProperties.ColorId, __instance.Image);

        __instance.Image.maskInteraction = __instance.matProperties.MaskType switch
        {
            PlayerMaterial.MaskType.SimpleUI or PlayerMaterial.MaskType.ScrollingUI => SpriteMaskInteraction.VisibleInsideMask,
            PlayerMaterial.MaskType.Exile => SpriteMaskInteraction.VisibleOutsideMask,
            _ => SpriteMaskInteraction.None
        };

        if (__instance.matProperties.MaskLayer > 0)
        {
            __instance.Image.material.SetInt(PlayerMaterial.MaskLayer, __instance.matProperties.MaskLayer);
            return false;
        }

        PlayerMaterial.SetMaskLayerBasedOnLocalPlayer(__instance.Image, __instance.matProperties.IsLocalPlayer);
        return false;
    }

    [HarmonyPatch(nameof(VisorLayer.SetFlipX))]
    [HarmonyPrefix]
    private static bool SetFlipXPrefix(VisorLayer __instance, bool flipX)
    {
        if (__instance.visorData == null || !__instance.visorData.ProductId.StartsWith("cmv_"))
            return true;

        __instance.Image.flipX = flipX;
        var asset = GetCache(__instance.visorData.name);
        if (asset != null)
            __instance.Image.sprite = flipX && asset.LeftIdleFrame ? asset.LeftIdleFrame : asset.IdleFrame;
        return false;
    }

    [HarmonyPatch(nameof(VisorLayer.SetVisor), [typeof(VisorData), typeof(int)])]
    [HarmonyPrefix]
    private static bool SetVisorPrefix(VisorLayer __instance, VisorData data, int color)
    {
        if (!data.ProductId.StartsWith("cmv_")) return true;
        __instance.visorData = data;
        var behind = CustomVisorManager.BehindHatsCache.TryGetValue(data.ProductId, out var b) && b;
        __instance.transform.SetLocalZ(behind
            ? __instance.ZIndexSpacing * -1.5f
            : __instance.ZIndexSpacing * -3f);
        __instance.SetFlipX(__instance.Image.flipX);
        __instance.SetMaterialColor(color);
        return false;
    }
}
