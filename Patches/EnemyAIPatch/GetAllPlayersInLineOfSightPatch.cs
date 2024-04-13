using DramaMask.Patches.Base;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace DramaMask.Patches.EnemyAIPatch;

[HarmonyPatch(typeof(EnemyAI), nameof(EnemyAI.GetAllPlayersInLineOfSight))]
public class GetAllPlayersInLineOfSightPatch : ModifyPlayerArrayPatch
{
    [HarmonyPrefix]
    public static void Prefix(EnemyAI __instance,
        float width = 45f, int range = 60, Transform eyeObject = null, float proximityCheck = -1f, int layerMask = -1)
    {
        SaveAndModifyPlayerArray(__instance);
    }

    // TODO: Transpile OOB

    [HarmonyPostfix]
    public static void Postfix(EnemyAI __instance, ref PlayerControllerB[] __result,
        float width = 45f, int range = 60, Transform eyeObject = null, float proximityCheck = -1f, int layerMask = -1)
    {
        LoadOriginalPlayerArray(__instance);
    }
}