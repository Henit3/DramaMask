using DramaMask.Extensions;
using DramaMask.Network;
using HarmonyLib;

namespace DramaMask.Patches.GrabbableObjectItemPatch;

[HarmonyPatch(typeof(GrabbableObject), "InteractLeftRightServerRpc")]
public class InteractLeftRightPatch
{
    private static bool _useInputFlipFlop = false;

    [HarmonyPostfix]
    public static void Postfix(GrabbableObject __instance, bool right)
    {
        if (__instance is not HauntedMaskItem instance) return;

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
                    if (Plugin.Config.UseStealthMeter.Value)
                    {
                        targetStealthData.SetLastStoppedStealthNow();
                    }
                    Traverse.Create(instance).Field("maskOn").SetValue(false);
                }
            }
            else
            {
                // Local invocation of behaviour so we can query hoveringOverTrigger (tied to camera)
                if (instance.playerHeldBy.IsLocal()
                    && instance.playerHeldBy.hoveringOverTrigger == null)
                {
                    targetPretendData.IsMaskEyesOn = !targetPretendData.IsMaskEyesOn;
                    NetworkHandler.Instance.SetPlayerMaskEyesValueServerRpc(id, targetPretendData.IsMaskEyesOn);
                }
            }
        }
        else
        {
            if (!right)
            {
                targetPretendData.IsMaskAttached = true;
                Traverse.Create(instance).Field("maskOn").SetValue(true);
            }
        }
    }
}
