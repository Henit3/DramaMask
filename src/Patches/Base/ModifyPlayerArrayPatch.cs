using DramaMask.Extensions;
using GameNetcodeStuff;
using HarmonyLib;
using System.Linq;
using System.Reflection.Emit;

namespace DramaMask.Patches.Base;

public abstract class ModifyPlayerArrayPatch
{
    private static PlayerControllerB[] allPlayerScriptsOriginal;

    protected static void AddOobCheckToLoopPredicate(CodeMatcher matcher, CodeInstruction index)
    {
        // Store the branch target to use in adjusted predicate
        var enterLoopTarget = matcher.Instruction.operand;

        // Replace blt with clt to only check as part of first "and" condition
        matcher.RemoveInstruction().InsertAndAdvance([new(OpCodes.Clt)]);

        // Add extra "and" condition to check if out of bounds, branching into processing loop if both satisfied
        // ... && IsWithinPlayerBounds(index)
        matcher.InsertAndAdvance(
            index,                          // index
            new(OpCodes.Call,               // IsWithinPlayerBounds()
                AccessTools.Method(typeof(ModifyPlayerArrayPatch), nameof(IsWithinPlayerBounds))),
            new(OpCodes.And),               // &&
            new(OpCodes.Brtrue,
                enterLoopTarget)            // enter processing loop
        );
    }

    // For accompanying OOB checking transpilation
    protected static bool IsWithinPlayerBounds(int index)
    {
        return index < StartOfRound.Instance.allPlayerScripts.Length;
    }

    protected static void SaveAndModifyPlayerArray(EnemyAI __instance)
    {
        if (!ShouldHideFromEnemy(__instance)) return;

        // Save old value and filter out searching for players that are activating a configured mask
        allPlayerScriptsOriginal = StartOfRound.Instance.allPlayerScripts;

        StartOfRound.Instance.allPlayerScripts = StartOfRound.Instance.allPlayerScripts
            .Where(player => player == null || !player.isPlayerControlled || player.IsHidden())
            .ToArray();
    }

    protected static void LoadOriginalPlayerArray(EnemyAI __instance)
    {
        if (!ShouldHideFromEnemy(__instance)) return;

        // Reset the player array to its old value
        StartOfRound.Instance.allPlayerScripts = allPlayerScriptsOriginal;
    }

    private static bool ShouldHideFromEnemy(EnemyAI __instance)
    {
        return Plugin.Config.HideFromAllEnemies
            || __instance is MaskedPlayerEnemy;
    }
}