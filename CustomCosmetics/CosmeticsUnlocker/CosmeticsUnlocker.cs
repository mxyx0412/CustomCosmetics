namespace CustomCosmetics.CosmeticsUnlocker;

internal static class CosmeticsUnlockerPatches
{
    private static void UnlockCosmetics(HatManager hatManager)
    {
        foreach (var bundle in hatManager.allBundles)
            bundle.Free = true;

        foreach (var bundle in hatManager.allFeaturedBundles)
            bundle.Free = true;

        foreach (var cube in hatManager.allFeaturedCubes)
            cube.Free = true;

        foreach (var item in hatManager.allFeaturedItems)
            item.Free = true;

        foreach (var hat in hatManager.allHats)
            hat.Free = true;

        foreach (var plate in hatManager.allNamePlates)
            plate.Free = true;

        foreach (var pet in hatManager.allPets)
            pet.Free = true;

        foreach (var skin in hatManager.allSkins)
            skin.Free = true;

        foreach (var bundle in hatManager.allStarBundles)
            bundle.price = 0;

        foreach (var visor in hatManager.allVisors)
            visor.Free = true;
    }

    [HarmonyPatch(typeof(HatManager), nameof(HatManager.Initialize))]
    public static class HatManager_Initialize
    {
        public static void Postfix(HatManager __instance)
        {
            if (Main.Unlocker.Value)
                UnlockCosmetics(__instance);
        }
    }
}
