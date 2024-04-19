using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace DramaMask.Patches.HauntedMaskItemPatch;

[HarmonyPatch(typeof(HauntedMaskItem), nameof(HauntedMaskItem.MaskClampToHeadAnimationEvent))]
public class MaskClampPatch
{
    [HarmonyPrefix]
    public static void Prefix(HauntedMaskItem __instance,
        bool ___attaching, PlayerControllerB ___previousPlayerHeldBy)
    {
        // Mimic the original condition for possession
        if (!___attaching || ___previousPlayerHeldBy == null) return;
        if (__instance.currentHeadMask == null) return;

        Object.Destroy(__instance.currentHeadMask.gameObject);
        __instance.currentHeadMask = null;
    }
}
