using DramaMask.Config;
using DramaMask.Extensions;
using DramaMask.Network;
using HarmonyLib;
using System;

namespace DramaMask.Patches.GrabbableObjectItemPatch;

[HarmonyPatch(typeof(GrabbableObject), "InteractLeftRightServerRpc")]
public class InteractLeftRightPatch
{
    private static bool _useInputFlipFlop = false;

    [HarmonyPostfix]
    public static void Postfix(GrabbableObject __instance, bool right)
    {
        if (__instance is not HauntedMaskItem instance
            || !instance.CanHide()) return;

        if (instance.playerHeldBy == null) return;

        // Continue only if the server (behaviour), or the local player (UI)
        if (!(NetworkHandler.IsHostOrServer() || instance.playerHeldBy.IsLocal())) return;

        var id = instance.playerHeldBy.GetId();

        // Gets invoked twice for host so only use first
        if (NetworkHandler.IsHostOrServer() && instance.playerHeldBy.IsLocal())
        {
            _useInputFlipFlop = !_useInputFlipFlop;
            if (_useInputFlipFlop) return;
        }

        var targetStealthData = NetworkHandler.Instance.GetStealth(instance.playerHeldBy.IsLocal(), id);
        if (targetStealthData == null) return;

        var targetPretendData = NetworkHandler.Instance.GetPretend(instance.playerHeldBy.IsLocal(), id);
        if (targetPretendData == null) return;

        if (targetPretendData.IsMaskAttached)
        {
            if (!right)
            {
                targetPretendData.IsMaskAttached = false;
                if (Plugin.Config.ChangeClientViewInstantly) instance.SetMaskAttached(false);
                // Redundant: !targetStealthData.IsAttemptingStealth()
                if (targetStealthData.IsHoldingMask)
                {
                    // Set holding mask animation state manually, ready to let go when mouse unpressed
                    instance.playerHeldBy.playerBodyAnimator.SetBool("HoldMask", true);
                    instance.playerHeldBy.playerBodyAnimator
                        .Play("HoldingItemsRightHand.HoldMaskToFace", 2, 1.0f);
                }
                else
                {
                    Traverse.Create(instance).Field("maskOn").SetValue(false);
                }
            }
            else
            {
                // Local invocation of behaviour so we can query hoveringOverTrigger (tied to camera)
                var isMaskEyeInteractClash = !StartOfRound.Instance.localPlayerUsingController;
                try { isMaskEyeInteractClash = InputUtilsCompat.IsMaskEyeInteractClash(); }
                catch (TypeLoadException) { }

                if (instance.playerHeldBy.IsLocal()
                    && (!isMaskEyeInteractClash
                        || instance.playerHeldBy.hoveringOverTrigger == null))
                {
                    targetPretendData.IsMaskEyesOn = !targetPretendData.IsMaskEyesOn;
                    if (Plugin.Config.ChangeClientViewInstantly) instance.SetMaskEyes(targetPretendData.IsMaskEyesOn);
                    NetworkHandler.Instance.SetPlayerMaskEyesValueServerRpc(id, targetPretendData.IsMaskEyesOn);
                }
            }
        }
        else
        {
            if (!right)
            {
                targetPretendData.IsMaskAttached = true;
                if (Plugin.Config.ChangeClientViewInstantly) instance.SetMaskAttached(true);
                Traverse.Create(instance).Field("maskOn").SetValue(true);
            }
        }
    }
}
