using HarmonyLib;

namespace DramaMask.Patches.HauntedMaskItemPatch;

[HarmonyPatch(typeof(HauntedMaskItem), nameof(HauntedMaskItem.PocketItem))]
public class PocketItemPatch
{
    [HarmonyPostfix]
    public static void Postfix(HauntedMaskItem __instance)
    {
        __instance.playerHeldBy.equippedUsableItemQE = false;
    }
}
