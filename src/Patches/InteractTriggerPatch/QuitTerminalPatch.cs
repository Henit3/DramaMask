using DramaMask.Extensions;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace DramaMask.Patches.PlayerControllerBPatch;

[HarmonyPatch(typeof(InteractTrigger), nameof(InteractTrigger.StopSpecialAnimation))]
public class StopSpecialAnimationPatch : BaseEndSpecialAnimationPatch
{
    [HarmonyPostfix]
    public static void Postfix(InteractTrigger __instance, Transform ___lockedPlayer)
    {
        if (___lockedPlayer == null) return;

        HideMaskVisibilityOnAnimationEnd(___lockedPlayer.GetComponent<PlayerControllerB>());
    }
}
