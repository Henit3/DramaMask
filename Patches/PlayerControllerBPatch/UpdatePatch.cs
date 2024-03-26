using DramaMask.Extensions;
using DramaMask.Network;
using GameNetcodeStuff;
using HarmonyLib;
using System;
using UnityEngine;

namespace DramaMask.Patches.PlayerControllerBPatch;

[HarmonyPatch(typeof(PlayerControllerB), "Update")]
public class UpdatePatch
{
    [HarmonyAfter("Sligili-More_Emotes-1.3.3")]
    [HarmonyPostfix]
    public static void Postfix(PlayerControllerB __instance)
    {
        //HandleAnimationOverride(__instance);
        HandleStealth(__instance);
    }

    // We aren't adding transitions or states, only overriding holdingMask, so no special extra initialisation
    private static void HandleAnimationOverride(PlayerControllerB __instance)
    {
        // Let MoreEmotes handle overriding the controller if it is detected
        // LethalEmotesApi and TooManyEmotes replace this on plugin awake so can be ignored
        if (Plugin.IsMoreEmotesPresent) return;

        // Quit if the controller has been overridden already
        var controller = __instance.playerBodyAnimator.runtimeAnimatorController;
        if (controller is AnimatorOverrideController) return;

        __instance.playerBodyAnimator.runtimeAnimatorController = new AnimatorOverrideController(controller);
    }

    private static void HandleStealth(PlayerControllerB __instance)
    {
        if (!ConfigValues.UseStealthMeter) return;

        // Ignore updates called by pre-loaded scripts that are not controlled by a player
        if (!__instance.isPlayerControlled) return;

        // Continue only if the server (behaviour), or the local player (UI)
        if (!(NetworkHandler.IsHostOrServer() || __instance.IsLocal())) return;

        var id = __instance.GetId();

        var targetData = __instance.IsLocal()
            ? NetworkHandler.Instance.MyStealth
            : NetworkHandler.Instance.StealthMap[id];

        var isAttemptingStealth = targetData.IsAttemptingStealth();
        if (isAttemptingStealth && targetData.StealthValue > 0)
        {
            targetData.StealthValue = Math.Max(0, targetData.StealthValue - Time.deltaTime);
        }
        else if (!isAttemptingStealth
            && targetData.StealthValue < ConfigValues.MaxHiddenTime
            && (!targetData.LastStoppedStealth.HasValue
                || DateTime.UtcNow.Subtract(targetData.LastStoppedStealth.Value)
                    .TotalSeconds > ConfigValues.RechargeDelay))
        {
            targetData.StealthValue = Math.Min(ConfigValues.MaxHiddenTime,
                targetData.StealthValue + Time.deltaTime);
            if (targetData.LastStoppedStealth != null) targetData.LastStoppedStealth = null;
            if (targetData.AddExhaustionPenalty) targetData.AddExhaustionPenalty = false;
        }
    }
}
