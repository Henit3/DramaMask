using DramaMask.Config;
using DramaMask.Extensions;
using GameNetcodeStuff;
using HarmonyLib;
using System.Diagnostics;

namespace DramaMask.Patches.Enemies.EnemyAIPatch;

[HarmonyPatch(typeof(EnemyAI), nameof(EnemyAI.PlayerIsTargetable))]
public class PlayerIsTargetablePatch
{
    [HarmonyPrefix]
    public static bool Prefix(EnemyAI __instance, ref bool __result,
        PlayerControllerB playerScript, bool cannotBeInShip = false, bool overrideInsideFactoryCheck = false)
    {
        if (playerScript.IsHidden() && !EnemyTargetHandler.ShouldCollideWithEnemy(__instance))
        {
            __result = false;
            return false;
        }

        if (!Plugin.Config.IncreaseCustomEnemyCompatibility.Value) return true;

        // Don't want to invalidate player collision
        var callingMethod = new StackFrame(1).GetMethod().Name;
        if (nameof(EnemyAI.MeetsStandardPlayerCollisionConditions) == callingMethod) return true;

        if (!playerScript.IsHidden()) return true;

        __result = false;
        return false;
    }
}