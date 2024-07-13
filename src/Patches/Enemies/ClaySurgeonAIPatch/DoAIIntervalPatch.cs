using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace DramaMask.Patches.Enemies.ClaySurgeonAIPatch;

[HarmonyPatch(typeof(ClaySurgeonAI), nameof(ClaySurgeonAI.DoAIInterval))]
public class DoAIIntervalPatch
{
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> HandleNonTargetablePlayer(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var matcher = new CodeMatcher(instructions);

        // Match for last check on targetPlayer (before re-assignment on failed TargetClosestPlayer)
        matcher.End();
        matcher.MatchBack(false, [new(OpCodes.Brfalse_S)]);
        var skipReassignmentTarget = matcher.Instruction.operand;

        // if (!PlayerIsTargetable(targetPlayer)) return;
        matcher.InsertAndAdvance([
            new(OpCodes.Ldloc_0),       // targetPlayer
            new(OpCodes.Call,           // IsThreatHiddenPlayer()
                AccessTools.Method(typeof(EnemyAI), nameof(EnemyAI.PlayerIsTargetable))),
            new(OpCodes.Brfalse,         // <skip next if branch>
                skipReassignmentTarget)
        ]);

        return matcher.InstructionEnumeration();
    }
}