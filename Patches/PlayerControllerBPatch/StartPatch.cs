using DramaMask.Extensions;
using GameNetcodeStuff;
using HarmonyLib;
using System.Linq;

namespace DramaMask.Patches.PlayerControllerBPatch;

[HarmonyPatch(typeof(PlayerControllerB), "Start")]
public class StartPatch
{
    /*[HarmonyPostfix]
    public static void Postfix(PlayerControllerB __instance)
    {
        if (HauntedMaskItemExtensions.HoldingMaskAnimation is not null) return;

        HauntedMaskItemExtensions.HoldingMaskAnimation = __instance
            .playerBodyAnimator.runtimeAnimatorController.animationClips
            .FirstOrDefault(anim => anim.name == "HoldMaskToFace");
    }*/
}
