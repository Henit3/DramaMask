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

        var targetStealthData = NetworkHandler.Instance.GetStealth(instance.playerHeldBy.IsLocal(), id);
        if (targetStealthData == null) return;

        var targetPretendData = NetworkHandler.Instance.GetPretend(instance.playerHeldBy.IsLocal(), id);
        if (targetPretendData == null) return;

        targetStealthData.IsHoldingMask = buttonDown && instance.CanHide();
        if (!targetPretendData.IsMaskAttached)
        {
            Traverse.Create(instance).Field("maskOn").SetValue(buttonDown);
        }

        if (!instance.playerHeldBy.IsLocal()) return;

        instance.SetMaskView(buttonDown
            ? (int)Plugin.Config.HeldMaskView.LocalValue
            : null);
    }
}
