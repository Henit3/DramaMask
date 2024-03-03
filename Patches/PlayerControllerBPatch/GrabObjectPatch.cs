using GameNetcodeStuff;
using HarmonyLib;

namespace DramaMask.Patches.PlayerControllerBPatch;

[HarmonyPatch(typeof(PlayerControllerB), "BeginGrabObject")]
public class GrabObjectPatch : BaseChangeItemPatch
{
    [HarmonyPrefix]
    public static bool Prefix(PlayerControllerB __instance)
    {
        return ShouldInvoke(__instance);
    }
}
