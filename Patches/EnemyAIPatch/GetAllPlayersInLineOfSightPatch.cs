using GameNetcodeStuff;
using HarmonyLib;

namespace DramaMask.Patches.EnemyAIPatch;

[HarmonyPatch(typeof(EnemyAI), nameof(EnemyAI.GetAllPlayersInLineOfSight))]
public class GetAllPlayersInLineOfSightPatch : BaseModifyPlayerArrayPatch
{
    [HarmonyPrefix]
    public static void Prefix(EnemyAI __instance,
        float width = 45f, int range = 60, int proximityAwareness = -1, float bufferDistance = 0f)
    {
        SaveAndModifyPlayerArray(__instance);
    }

    [HarmonyPostfix]
    public static void Postfix(EnemyAI __instance, ref PlayerControllerB __result,
        float width = 45f, int range = 60, int proximityAwareness = -1, float bufferDistance = 0f)
    {
        LoadOriginalPlayerArray(__instance);
    }
}