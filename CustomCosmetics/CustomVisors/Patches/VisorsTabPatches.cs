using AmongUs.Data;
using Cpp2IL.Core.Extensions;
using CustomCosmetics.Core;
using Innersloth.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace CustomCosmetics.CustomVisors.Patches;

[HarmonyPatch(typeof(VisorsTab))]
internal static class VisorsTabPatches
{
    private static TextMeshPro _textTemplate;
    private static bool _injected;

    private static void EnsureInjectedVisors(HatManager hm)
    {
        // tab fallback: also injects when GetVisorById was never called (e.g. visors disabled at startup)
        if (_injected || CustomVisorManager.UnregisteredVisors.Count == 0) return;

        try
        {
            var all = hm.allVisors != null ? hm.allVisors.ToList() : new List<VisorData>();
            var cache = CustomVisorManager.UnregisteredVisors.Clone();
            foreach (var vc in cache)
            {
                try
                {
                    all.Add(CustomVisorManager.CreateVisorBehaviour(vc));
                    CustomVisorManager.UnregisteredVisors.Remove(vc);
                }
                catch { }
            }
            hm.allVisors = all.ToArray();
            if (CustomVisorManager.UnregisteredVisors.Count == 0) _injected = true;
        }
        catch (Exception ex)
        {
            Warn($"EnsureInjectedVisors failed: {ex}");
        }
    }

    [HarmonyPatch(nameof(VisorsTab.OnEnable))]
    [HarmonyPrefix]
    private static void OnEnablePrefix(VisorsTab __instance)
    {
        try
        {
            var hatManager = DestroyableSingleton<HatManager>.Instance;
            if (hatManager == null) return;
            EnsureInjectedVisors(hatManager);
        }
        catch (Exception ex)
        {
            Warn($"VisorsTab prefix failed: {ex}");
        }
    }

    [HarmonyPatch(nameof(VisorsTab.OnEnable))]
    [HarmonyPrefix]
    private static bool RebuildPrefix(VisorsTab __instance)
    {
        try
        {
            var hatManager = DestroyableSingleton<HatManager>.Instance;
            if (hatManager == null) return false;
            EnsureInjectedVisors(hatManager);

            var allVisors = hatManager.allVisors;
            if (allVisors == null) return false;

            var packages = new Dictionary<string, List<VisorData>>();
            foreach (var v in allVisors)
            {
                if (v == null) continue;
                var pkg = CustomVisorManager.PackageCache.TryGetValue(v.name, out var p) ? p : "Innersloth";
                if (!packages.ContainsKey(pkg))
                    packages[pkg] = new List<VisorData>();
                packages[pkg].Add(v);
            }

            // stop vanilla preview coroutines before destroying chips (avoid NRE on destroyed objects)
            __instance.StopAllCoroutines();
            Core.Helpers.DestroyChildren(__instance.scroller.Inner);
            __instance.ColorChips = new ISystem.List<ColorChip>();

            var yOffset = __instance.YStart - 0.5f * __instance.YOffset;
            _textTemplate = Core.Helpers.GetTitleTemplate(__instance, ref _textTemplate);

            var orderedKeys = Core.Helpers.OrderPackageKeys(packages.Keys, "Innersloth",
                CustomVisorManager.PackagePriorities);
            foreach (var key in orderedKeys)
            {
                var list = packages[key];
                InsertNoVisor(list, hatManager);
                yOffset = CreatePackage(list, key, yOffset, __instance);
            }

            __instance.scroller.ContentYBounds.max = -(yOffset + 4.1f);
            return false;
        }
        catch (Exception ex)
        {
            Error($"VisorsTab rebuild failed: {ex}");
            return false;
        }
    }

    private static void InsertNoVisor(List<VisorData> visors, HatManager hm)
    {
        // empty visor at head of every package (vanilla visor_EmptyVisor)
        if (visors.Count > 0 && visors[0].ProductId == "visor_EmptyVisor") return;
        var noVisor = hm.GetVisorById("visor_EmptyVisor");
        if (noVisor != null)
            visors.Insert(0, noVisor);
    }

    private static float CreatePackage(List<VisorData> visors, string pkg, float yStart, VisorsTab tab)
    {
        var isVanilla = pkg == "Innersloth";
        var offset = yStart;

        if (_textTemplate != null)
        {
            var title = UObject.Instantiate(_textTemplate, tab.scroller.Inner);
            title.gameObject.SetActive(true);
            title.rectTransform.sizeDelta = new Vector2(5f, 1f);
            title.transform.localPosition = new Vector3(tab.XRange.Lerp(0.5f), yStart + 0.4f * tab.YOffset, -1f);
            title.alignment = TextAlignmentOptions.Center;
            title.transform.localScale = Vector3.one * 1.5f;
            title.fontSize *= 0.5f;
            title.enableAutoSizing = false;
            var displayName = isVanilla
                ? pkg
                : (CustomVisorManager.PackageDisplayNames.TryGetValue(pkg, out var dn) ? dn : pkg);
            tab.StartCoroutine(Effects.Lerp(0.1f, new Action<float>(_ => title.SetText(displayName))));
            offset -= 0.6f * tab.YOffset;
        }

        for (var i = 0; i < visors.Count; i++)
        {
            var v = visors[i];
            var xPos = tab.XRange.Lerp(i % tab.NumPerRow / (tab.NumPerRow - 1f));
            var yPos = offset - (i / tab.NumPerRow * tab.YOffset);
            var chip = UObject.Instantiate(tab.ColorTabPrefab, tab.scroller.Inner);
            // vanilla VisorsTab.Update compares chip.ProductId
            chip.ProductId = v.ProductId;

            if (ActiveInputManager.currentControlType == ActiveInputManager.InputType.Keyboard)
            {
                chip.Button.OnMouseOver.AddListener((Action)(() => tab.SelectVisor(v)));
                chip.Button.OnMouseOut.AddListener((Action)(() =>
                    tab.SelectVisor(DestroyableSingleton<HatManager>.Instance.GetVisorById(DataManager.Player.Customization.Visor))));
                chip.Button.OnClick.AddListener((Action)tab.ClickEquip);
            }
            else
            {
                chip.Button.OnClick.AddListener((Action)(() => tab.SelectVisor(v)));
            }
            chip.Button.ClickMask = tab.scroller.Hitbox;
            if (chip.Inner != null)
            {
                chip.Inner.SetMaskType(PlayerMaterial.MaskType.ScrollingUI);
                tab.UpdateMaterials(chip.Inner.FrontLayer, v);
            }

            chip.transform.localPosition = new Vector3(xPos, yPos, -1f);
            var sr = FindVisorSprite(chip);
            if (sr != null)
            {
                if (CustomVisorManager.ViewDataCache.TryGetValue(v.name, out var vd))
                {
                    sr.sprite = vd.IdleFrame;
                    sr.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
                }
                else
                {
                    var srRef = sr;
                    tab.StartCoroutine(AddressableAssetExtensions.CoLoadAssetAsync<VisorViewData>(tab,
                        v.ViewDataRef,
                        (Action<VisorViewData>)(vd2 =>
                        {
                            if (vd2 != null && srRef != null)
                            {
                                srRef.sprite = vd2.IdleFrame;
                                srRef.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
                            }
                        })));
                }
            }
            chip.Tag = v;
            chip.SelectionHighlight.gameObject.SetActive(false);
            tab.ColorChips.Add(chip);
        }

        return offset - ((visors.Count - 1) / tab.NumPerRow * tab.YOffset) - 1.75f;
    }

    private static SpriteRenderer FindVisorSprite(ColorChip chip)
    {
        // visor chip has no Inner(HatParent); sprite lives at GetChild(2).GetChild(0)
        if (chip == null) return null;
        var root = chip.transform;

        if (root.childCount > 2)
        {
            var c2 = root.GetChild(2);
            if (c2 != null && c2.childCount > 0)
            {
                var sr = c2.GetChild(0).GetComponent<SpriteRenderer>();
                if (sr != null) return sr;
            }
        }

        foreach (var sr in root.GetComponentsInChildren<SpriteRenderer>(true))
            if (sr != null && sr.sprite == null) return sr;

        var all = root.GetComponentsInChildren<SpriteRenderer>(true);
        return all.Length > 0 ? all[all.Length - 1] : null;
    }

    [HarmonyPatch(nameof(VisorsTab.SelectVisor))]
    [HarmonyPostfix]
    private static void SelectVisorPostfix(VisorData visor)
    {
        // item name: custom visors show "{name}\nby {author}"
        Core.Helpers.SetCustomItemName(visor.ProductId, visor.name, "cmv_", CustomVisorManager.AuthorCache);
    }
}
