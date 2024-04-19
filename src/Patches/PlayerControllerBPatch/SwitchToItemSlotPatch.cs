using GameNetcodeStuff;
using HarmonyLib;

namespace DramaMask.Patches.PlayerControllerBPatch;

[HarmonyPatch(typeof(PlayerControllerB), "SwitchToItemSlot")]
public class SwitchToItemSlotPatch : BaseChangeItemPatch
{
    [HarmonyPrefix]
    public static bool Prefix(PlayerControllerB __instance)
    {
        return ShouldInvoke(__instance);
    }
}
