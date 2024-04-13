using DramaMask.Extensions;
using GameNetcodeStuff;
using HarmonyLib;
using System.Diagnostics;

namespace DramaMask.Patches.EnemyAIPatch;

[HarmonyPatch(typeof(EnemyAI), nameof(EnemyAI.PlayerIsTargetable))]
public class PlayerIsTargetablePatch
{
    [HarmonyPrefix]
    public static bool Prefix(EnemyAI __instance, ref bool __result, 
        PlayerControllerB playerScript, bool cannotBeInShip = false, bool overrideInsideFactoryCheck = false)
    {
        // Don't want to invalidate player collision
        var callingMethod = new StackFrame(1).GetMethod().Name;
        if (nameof(EnemyAI.MeetsStandardPlayerCollisionConditions) == callingMethod) return true;

        if (!playerScript.IsHidden()) return true;

        __result = false;
        return false;
    }
}