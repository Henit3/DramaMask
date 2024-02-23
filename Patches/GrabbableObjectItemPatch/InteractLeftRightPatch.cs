using DramaMask.Extensions;
using DramaMask.Network;
using HarmonyLib;
using System.Collections.Generic;

namespace DramaMask.Patches.GrabbableObjectItemPatch;

[HarmonyPatch(typeof(GrabbableObject), "InteractLeftRightServerRpc")]
public class InteractLeftRightPatch
{
    private static Dictionary<ulong, bool> _useInputFlipFlop = new();

    [HarmonyPostfix]
    public static void Postfix(GrabbableObject __instance, bool right)
    {
        if (__instance is not HauntedMaskItem instance) return;

        if (instance.playerHeldBy == null) return;

        // Continue only if the server (behaviour), or the local player (UI)
        if (!(NetworkHandler.IsHostOrServer() || instance.playerHeldBy.IsLocal())) return;

        var id = instance.playerHeldBy.GetId();

        // Gets invoked twice so only use first (InteractQE, SecondaryUse)
        if (NetworkHandler.IsHostOrServer())
        {
            _useInputFlipFlop.AddSafe(id, false);
            _useInputFlipFlop[id] = !_useInputFlipFlop[id];
            if (_useInputFlipFlop[id]) return;
        }

        var targetStealthData = instance.playerHeldBy.IsLocal()
            ? NetworkHandler.Instance.MyStealth
            : NetworkHandler.Instance.StealthMap[id];

        var targetPretendData = instance.playerHeldBy.IsLocal()
            ? NetworkHandler.Instance.MyPretend
            : NetworkHandler.Instance.PretendMap[id];

        if (targetPretendData.IsMaskAttached)
        {
            if (!right)
            {
                targetPretendData.IsMaskAttached = false;
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
                    if (ConfigValues.UseStealthMeter)
                    {
                        targetStealthData.SetLastStoppedStealthNow();
                    }
                }
            }
            else
            {
                targetPretendData.IsMaskEyesOn = !targetPretendData.IsMaskEyesOn;
            }
        }
        else
        {
            if (!right)
            {
                targetPretendData.IsMaskAttached = true;
            }
        }
    }
}
