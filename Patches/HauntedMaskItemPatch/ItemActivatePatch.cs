﻿using DramaMask.Extensions;
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
        if (!(NetworkHandler.IsHostOrServer() || instance.playerHeldBy.IsLocal())) return;

        var id = instance.playerHeldBy.GetId();

        var targetStealthData = instance.playerHeldBy.IsLocal()
            ? NetworkHandler.Instance.MyStealth
            : NetworkHandler.Instance.StealthMap[id];

        var targetPretendData = instance.playerHeldBy.IsLocal()
            ? NetworkHandler.Instance.MyPretend
            : NetworkHandler.Instance.PretendMap[id];

        if (!targetPretendData.IsMaskAttached)
        {
            targetStealthData.IsAttemptingStealth = buttonDown && instance.CanHide();
            if (ConfigValues.UseStealthMeter && !buttonDown)
            {
                var adjustedTime = DateTime.UtcNow;
                if (targetStealthData.AddExhaustionPenalty) adjustedTime = adjustedTime.AddSeconds(ConfigValues.ExhaustionPenaltyDelay);
                targetStealthData.LastStoppedStealth = adjustedTime;
            }

            if (!instance.playerHeldBy.IsLocal()) return;

            instance.SetOutlineView(buttonDown);
        }
        else
        {
            targetPretendData.IsRaisingArms = buttonDown;
        }
    }
}
