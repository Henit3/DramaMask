using DramaMask.Extensions;
using HarmonyLib;

namespace DramaMask.Patches.CentipedeAIPatch;

[HarmonyPatch(typeof(CentipedeAI), nameof(CentipedeAI.TriggerCentipedeFallServerRpc))]
public class TriggerCentipedeFallPatch
{
    [HarmonyPrefix]
    public static bool Prefix(CentipedeAI __instance, ref bool ___triggeredFall,
        ulong clientId)
    {
        if (!GameNetworkManager.Instance.localPlayerController.IsHidden()) return true;

        // Reset the triggeredFall state if cancelling the fall
        ___triggeredFall = false;
        return false;
    }
}