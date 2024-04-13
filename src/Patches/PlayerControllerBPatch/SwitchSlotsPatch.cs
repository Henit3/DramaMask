using GameNetcodeStuff;
using HarmonyLib;

namespace DramaMask.Patches.PlayerControllerBPatch;

[HarmonyPatch(typeof(PlayerControllerB), "ScrollMouse_performed")]
public class SwitchSlotsPatch : BaseChangeItemPatch
{
    [HarmonyPrefix]
    public static bool Prefix(PlayerControllerB __instance)
    {
        if (__instance.inTerminalMenu) return true;

        return ShouldInvoke(__instance);
    }
}
