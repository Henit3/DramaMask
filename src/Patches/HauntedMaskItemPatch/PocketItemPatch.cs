using HarmonyLib;

namespace DramaMask.Patches.HauntedMaskItemPatch;

[HarmonyPatch(typeof(HauntedMaskItem), nameof(HauntedMaskItem.PocketItem))]
public class PocketItemPatch
{
    [HarmonyPrefix]
    public static void Prefix(HauntedMaskItem __instance)
    {
        if (__instance.playerHeldBy == null) return;

        __instance.playerHeldBy.equippedUsableItemQE = false;
    }
}
