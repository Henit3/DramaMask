using HarmonyLib;

namespace DramaMask.Patches.HauntedMaskItemPatch;

[HarmonyPatch(typeof(HauntedMaskItem), nameof(HauntedMaskItem.EquipItem))]
public class EquipItemPatch
{
    [HarmonyPostfix]
    public static void Postfix(HauntedMaskItem __instance)
    {
        __instance.playerHeldBy.equippedUsableItemQE = true;
    }
}
