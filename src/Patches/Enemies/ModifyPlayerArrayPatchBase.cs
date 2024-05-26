using DramaMask.Config;
using DramaMask.Extensions;
using GameNetcodeStuff;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace DramaMask.Patches.Enemies;

public abstract class ModifyPlayerArrayPatchBase
{
    private static PlayerControllerB[] allPlayerScriptsOriginal;

    // We go into the loop and add instructions at the start to maintain compatibility with other mods' transpilations checks
    protected static void AddOobCheckToLoopPredicate(CodeMatcher matcher, ILGenerator generator, CodeInstruction index)
    {
        // Store the current position to return back to after processing
        var predicatePosition = matcher.Pos;

        // Store the branch target to use in adjusted predicate
        if (!matcher.Instruction.Branches(out var possibleEnterLoopTarget))
        {
            Plugin.Logger.LogWarning("Couldn't apply OOB check to loop predicate: Loop entry target not accessible");
            return;
        }
        var enterLoopTarget = possibleEnterLoopTarget.Value;

        // Create a label on exiting the loop to jump out to if out of bounds, using pre-exisiting ones if available
        Label exitLoopTarget;
        matcher.Advance(1);
        if ((exitLoopTarget = matcher.Instruction.labels.FirstOrDefault()) == default)
        {
            exitLoopTarget = generator.DefineLabel();
            matcher.AddLabelsAt(matcher.Pos, [exitLoopTarget]);
        }

        // Go to the loop entry target to insert our checks
        matcher.Start();
        matcher.SearchForward((instruction) => instruction.labels.Contains(enterLoopTarget));

        // Jump back out of the loop if out of bounds
        List<CodeInstruction> insertedInstructions = [
            index,                          // index
            new(OpCodes.Call,               // IsWithinPlayerBounds()
                AccessTools.Method(typeof(ModifyPlayerArrayPatchBase), nameof(IsWithinPlayerBounds))),
            new(OpCodes.Brfalse,
                exitLoopTarget)            // exit processing loop
        ];

        // Insert checks and transfer the label from the old loop entry instruction to our new one
        var oldLoopEntryInstruction = matcher.Instruction;
        matcher.Insert(insertedInstructions);
        matcher.Instruction.MoveLabelsFrom(oldLoopEntryInstruction);

        // Return to starting instruction for further processing
        matcher.Start();
        matcher.Advance(predicatePosition + insertedInstructions.Count);
    }

    // For accompanying OOB checking transpilation
    protected static bool IsWithinPlayerBounds(int index)
    {
        return index < StartOfRound.Instance.allPlayerScripts.Length;
    }

    // For getting the actual index after the hidden players have been removed
    protected static ulong GetTrueIndex(int index)
    {
        return StartOfRound.Instance.allPlayerScripts[index].playerClientId;
    }

    protected static void SaveAndModifyPlayerArray(EnemyAI __instance)
    {
        if (!EnemyTargetHandler.ShouldHideFromEnemy(__instance)) return;

        // Save old value and filter out searching for players that are activating a configured mask
        allPlayerScriptsOriginal = StartOfRound.Instance.allPlayerScripts;

        // Intentionally keep the invalid ones (null and non-controlled) for max compatibility
        StartOfRound.Instance.allPlayerScripts = StartOfRound.Instance.allPlayerScripts
            .Where(player => player == null || !player.isPlayerControlled || !player.IsHidden())
            .ToArray();
    }

    protected static void LoadOriginalPlayerArray(EnemyAI __instance)
    {
        if (!EnemyTargetHandler.ShouldHideFromEnemy(__instance)) return;

        // Reset the player array to its old value
        StartOfRound.Instance.allPlayerScripts = allPlayerScriptsOriginal;
    }
}