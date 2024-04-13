using DramaMask.Extensions;
using HarmonyLib;

namespace DramaMask.Patches.NutcrackerEnemyAIPatch;

[HarmonyPatch(typeof(NutcrackerEnemyAI), nameof(NutcrackerEnemyAI.CheckLineOfSightForLocalPlayer))]
public class CheckLineOfSightForLocalPlayerPatch
{
    [HarmonyPrefix]
    public static bool Prefix(NutcrackerEnemyAI __instance, ref bool __result,
        float width = 45f, int range = 60, int proximityAwareness = -1)
    {
        if (!GameNetworkManager.Instance.localPlayerController.IsHidden()) return true;

        // Override with can't find the player if the player is hiding
        __result = false;
        return false;
    }
}