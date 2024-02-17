using HarmonyLib;

namespace DramaMask.Patches.HauntedMaskItemPatch;

[HarmonyPatch(typeof(HauntedMaskItem), nameof(HauntedMaskItem.DiscardItem))]
public class DiscardItemPatch
{
    [HarmonyPostfix]
    public static void Postfix(HauntedMaskItem __instance)
    {
        __instance.playerHeldBy.equippedUsableItemQE = false;
    }
}
