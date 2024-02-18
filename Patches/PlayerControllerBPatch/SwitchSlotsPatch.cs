using DramaMask.Extensions;
using DramaMask.Network;
using GameNetcodeStuff;
using HarmonyLib;

namespace DramaMask.Patches.PlayerControllerBPatch;

[HarmonyPatch(typeof(PlayerControllerB), "ScrollMouse_performed")]
public class SwitchSlotsPatch
{
    [HarmonyPrefix]
    public static bool Prefix(PlayerControllerB __instance)
    {
        if (!__instance.isPlayerControlled) return true;

        if (!__instance.IsLocal()) return true;

        if (!NetworkHandler.Instance.MyPretend.IsMaskAttached) return true;

        // Cancel event if all of the above conditions are passed
        return false;
    }
}
