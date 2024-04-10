using DramaMask.Extensions;
using DramaMask.Network;
using HarmonyLib;

namespace DramaMask.Patches.CentipedeAIPatch;

[HarmonyPatch(typeof(CentipedeAI), nameof(CentipedeAI.TriggerCentipedeFallServerRpc))]
public class TriggerCentipedeFallPatch
{
    [HarmonyPrefix]
    public static bool Prefix(CentipedeAI __instance, ref bool ___triggeredFall,
        ulong clientId)
    {
        if (NetworkHandler.Instance.VisiblePlayers != null
            && !NetworkHandler.Instance.VisiblePlayers
                .Contains(GameNetworkManager.Instance.localPlayerController.GetId()))
        {
            // Reset the triggeredFall state if cancelling the fall
            ___triggeredFall = false;
            return false;
        }

        return true;
    }
}