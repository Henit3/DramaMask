using GameNetcodeStuff;
using HarmonyLib;

namespace DramaMask.Patches.PlayerControllerBPatch;

[HarmonyPatch(typeof(PlayerControllerB), "Discard_performed")]
public class DiscardItemPatch : BaseChangeItemPatch
{
    [HarmonyPrefix]
    public static bool Prefix(PlayerControllerB __instance)
    {
        return ShouldInvoke(__instance);
    }
}
