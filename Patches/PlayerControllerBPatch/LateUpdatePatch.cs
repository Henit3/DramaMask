using DramaMask.Extensions;
using DramaMask.Network;
using DramaMask.UI;
using GameNetcodeStuff;
using HarmonyLib;
using System;

namespace DramaMask.Patches.PlayerControllerBPatch;

[HarmonyPatch(typeof(PlayerControllerB), "LateUpdate")]
public class LateUpdatePatch
{
    [HarmonyPostfix]
    public static void Postfix(PlayerControllerB __instance)
    {
        HandleStealthMeter(__instance);
        HandleAttachedMask(__instance);
    }

    private static void HandleStealthMeter(PlayerControllerB __instance)
    {
        if (!ConfigValues.UseStealthMeter || !ConfigValues.SeeStealthMeter) return;

        // Ignore updates called by pre-loaded scripts that are not controlled by a player
        if (!__instance.isPlayerControlled) return;

        // There is only one stealth meter instance per client (not player)
        // Update only if this is being called on the local player object, and they are not dead
        if (!__instance.IsLocal() || __instance.isPlayerDead) return;

        var shouldBeVisible = ShouldBarBeVisible(__instance.currentlyHeldObjectServer);
        StealthMeterUI.Instance.Visible = shouldBeVisible;
        if (!shouldBeVisible) return;

        var percent = Math.Max(0, NetworkHandler.Instance.MyStealth.StealthValue) / ConfigValues.MaxHiddenTime;
        StealthMeterUI.Instance.UpdatePercentage(percent);
    }

    private static bool ShouldBarBeVisible(GrabbableObject heldItem)
    {
        // Visible if we can always see the meter, or we are holding a hiding mask
        return ConfigValues.AlwaysSeeStealthMeter
            || (heldItem is HauntedMaskItem item && item.CanHide());
    }

    private static void HandleAttachedMask(PlayerControllerB __instance)
    {
        // Should see all players if they do this
        if (!__instance.isPlayerControlled) return;

        if (__instance.currentlyHeldObjectServer == null
            || __instance.currentlyHeldObjectServer is not HauntedMaskItem mask) return;

        if (mask.currentHeadMask == null) return;

        AccessTools.Method(typeof(HauntedMaskItem), "PositionHeadMaskWithOffset").Invoke(mask, null);
    }
}
