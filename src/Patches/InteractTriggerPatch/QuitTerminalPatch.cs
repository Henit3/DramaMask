using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace DramaMask.Patches.PlayerControllerBPatch;

[HarmonyPatch(typeof(InteractTrigger), nameof(InteractTrigger.StopSpecialAnimation))]
public class StopSpecialAnimationPatch : BaseEndSpecialAnimationPatch
{
    [HarmonyPrefix]
    public static void Prefix(InteractTrigger __instance, Transform ___lockedPlayer, out Transform __state)
    {
        __state = ___lockedPlayer;
    }
    [HarmonyPostfix]
    public static void Postfix(InteractTrigger __instance, Transform __state)
    {
        if (__state == null) return;

        HideMaskVisibilityOnAnimationEnd(__state.GetComponent<PlayerControllerB>());
    }
}
