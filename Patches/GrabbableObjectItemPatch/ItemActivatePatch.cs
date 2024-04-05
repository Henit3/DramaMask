using DramaMask.Extensions;
using DramaMask.Network;
using HarmonyLib;

namespace DramaMask.Patches.GrabbableObjectItemPatch;

[HarmonyPatch(typeof(GrabbableObject), "ActivateItemServerRpc")]
public class ItemActivatePatch
{
    private static bool _useInputFlipFlop = false;

    [HarmonyPostfix]
    public static void Postfix(GrabbableObject __instance, bool onOff, bool buttonDown)
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

        targetStealthData.IsHoldingMask = buttonDown && instance.CanHide();
        if (!targetPretendData.IsMaskAttached)
        {
            // Redundant: !targetStealthData.IsAttemptingStealth() && 
            if (Plugin.Config.UseStealthMeter.Value && !buttonDown)
            {
                targetStealthData.SetLastStoppedStealthNow();
            }
            Traverse.Create(instance).Field("maskOn").SetValue(buttonDown);
        }
        else
        {
            targetPretendData.IsRaisingArms = buttonDown;
        }

        if (!instance.playerHeldBy.IsLocal()) return;

        if (!targetPretendData.IsMaskAttached)
        {
            instance.SetMaskView(buttonDown
                ? Plugin.Config.HeldMaskView.LocalValue
                : null);
        }
    }
}
