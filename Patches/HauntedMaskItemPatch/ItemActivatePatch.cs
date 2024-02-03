using DramaMask.Extensions;
using DramaMask.Network;
using HarmonyLib;
using System;

namespace DramaMask.Patches.HauntedMaskItemPatch;

[HarmonyPatch(typeof(GrabbableObject), "ActivateItemServerRpc")]
public class ItemActivatePatch
{
    [HarmonyPostfix]
    public static void Postfix(GrabbableObject __instance, bool onOff, bool buttonDown)
    {
        if (__instance is not HauntedMaskItem instance) return;

        if (instance.playerHeldBy == null) return;

        // Continue only if the server (behaviour), or the local player (UI)
        if (!(NetworkHandler.IsHostOrServer() || __instance.playerHeldBy.IsLocal())) return;

        var id = instance.playerHeldBy.GetId();

        var targetData = __instance.playerHeldBy.IsLocal()
            ? NetworkHandler.Instance.MyStealth
            : NetworkHandler.Instance.StealthMap[id];

        targetData.IsAttemptingStealth = buttonDown && instance.CanHide();
        if (ConfigValues.UseStealthMeter && !buttonDown)
        {
            var adjustedTime = DateTime.UtcNow;
            if (targetData.AddExhaustionPenalty) adjustedTime = adjustedTime.AddSeconds(ConfigValues.ExhaustionPenaltyDelay);
            targetData.LastStoppedStealth = adjustedTime;
        }
    }
}
