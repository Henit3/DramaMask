using DramaMask.Extensions;
using DramaMask.Network;
using HarmonyLib;
using System.Collections.Generic;

namespace DramaMask.Patches.GrabbableObjectItemPatch;

[HarmonyPatch(typeof(GrabbableObject), "ActivateItemServerRpc")]
public class ItemActivatePatch
{
    private static Dictionary<ulong, bool> _useInputFlipFlop = new();

    [HarmonyPostfix]
    public static void Postfix(GrabbableObject __instance, bool onOff, bool buttonDown)
    {
        if (__instance is not HauntedMaskItem instance) return;

        if (instance.playerHeldBy == null) return;

        // Continue only if the server (behaviour), or the local player (UI)
        if (!(NetworkHandler.IsHostOrServer() || instance.playerHeldBy.IsLocal())) return;

        var id = instance.playerHeldBy.GetId();

        // Gets invoked twice so only use first (performed / cancelled via server+client)
/// Might only apply to server, in which case we need simple bool logic
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

        targetStealthData.IsHoldingMask = buttonDown && instance.CanHide();
        if (!targetPretendData.IsMaskAttached)
        {
            // Redundant: !targetStealthData.IsAttemptingStealth() && 
            if (ConfigValues.UseStealthMeter && !buttonDown)
            {
                targetStealthData.SetLastStoppedStealthNow();
            }
        }
        else
        {
            targetPretendData.IsRaisingArms = buttonDown;
        }

        if (!instance.playerHeldBy.IsLocal()) return;

        instance.SetOutlineView(buttonDown);
    }
}
