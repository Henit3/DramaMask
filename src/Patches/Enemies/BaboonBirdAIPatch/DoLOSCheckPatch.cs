using DramaMask.Constants;
using DramaMask.Extensions;
using GameNetcodeStuff;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace DramaMask.Patches.Enemies.BaboonBirdAIPatch;

[HarmonyPatch(typeof(BaboonBirdAI), "DoLOSCheck")]
public class DoLOSCheckPatch
{
    private static bool IsThreatHiddenPlayer(IVisibleThreat threat)
    {
        return EnemyTargets.ShouldHideFromEnemy(nameof(BaboonBirdAI))
            && threat is PlayerControllerB player
            && player.IsHidden();
    }

    // Should only be called by the server so safe to use NetworkHandler.Instance.StealthMap
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> HidePlayerFromCollider(IEnumerable<CodeInstruction> instructions,
        ILGenerator generator)
    {
        var matcher = new CodeMatcher(instructions);

        // Match for the first local (visibility), at the start of threat checks
        matcher.MatchForward(false, [new(OpCodes.Ldloc_0)]);

        // Match for the next reference to "this", to get continue target above it
        matcher.MatchForward(false, [new(OpCodes.Ldarg_0)]);
        matcher.Advance(-1);
        var skipPlayerTarget = matcher.Instruction.operand;

        // Go back to start of threat checks to insert out player hidden check
        matcher.MatchBack(false, [new(OpCodes.Ldloc_0)]);
        matcher.Advance(3);
        // if (visibleThreat is PlayerControllerB player && player.IsHidden()) continue;
        matcher.InsertAndAdvance([
            new(OpCodes.Ldloc_0),       // visibleThreat
            new(OpCodes.Call,           // IsThreatHiddenPlayer()
                AccessTools.Method(typeof(DoLOSCheckPatch), nameof(IsThreatHiddenPlayer))),
            new(OpCodes.Brtrue,         // <skip next if branch>
                skipPlayerTarget)
        ]);

        return matcher.InstructionEnumeration();
    }
}