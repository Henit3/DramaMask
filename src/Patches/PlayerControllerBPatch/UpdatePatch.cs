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
    [HarmonyPostfix]
    public static void Postfix(PlayerControllerB __instance)
    {
        if (!Plugin.Config.UseStealthMeter.Value) return;

        // Ignore updates called by pre-loaded scripts that are not controlled by a player
        if (!__instance.isPlayerControlled) return;

        // Continue only if the server (behaviour), or the local player (UI)
        if (!(NetworkHandler.IsHostOrServer() || __instance.IsLocal())) return;

        var id = __instance.GetId();

        var pretendData = NetworkHandler.Instance.GetPretend(__instance.IsLocal(), id);
        if (pretendData == null) return;

        var stealthData = NetworkHandler.Instance.GetStealth(__instance.IsLocal(), id);
        if (stealthData == null) return;

        var isAttemptingStealth = stealthData.IsAttemptingStealth();
        if (isAttemptingStealth)
        {
            if (stealthData.StealthValue > 0)
            {
                stealthData.StealthValue = Math.Max(0, stealthData.StealthValue - (Time.deltaTime
                    * (pretendData.IsMaskAttached ? Plugin.Config.AttachedStealthMultiplier.Value : 1f)));
            }
            else
            {
                if (Plugin.Config.RemoveOnDepletion.Value)
                {
                    pretendData.IsMaskAttached = false;
                }
            }
        }
        else if (!isAttemptingStealth
            && stealthData.StealthValue < Plugin.Config.MaxHiddenTime.Value
            && (!stealthData.LastStoppedStealth.HasValue
                || DateTime.UtcNow.Subtract(stealthData.LastStoppedStealth.Value)
                    .TotalSeconds > Plugin.Config.RechargeDelay.Value))
        {
            stealthData.StealthValue = Math.Min(Plugin.Config.MaxHiddenTime.Value,
                stealthData.StealthValue + Time.deltaTime);
            if (stealthData.LastStoppedStealth != null) stealthData.LastStoppedStealth = null;
            if (stealthData.AddExhaustionPenalty) stealthData.AddExhaustionPenalty = false;
        }
    }
}
