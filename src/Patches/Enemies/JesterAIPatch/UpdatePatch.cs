using DramaMask.Extensions;
using GameNetcodeStuff;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace DramaMask.Patches.Enemies.JesterAIPatch;

[HarmonyPatch(typeof(JesterAI), nameof(JesterAI.Update))]
public class UpdatePatch
{
    // Do not check if the player is hidden
    private static readonly List<CodeInstruction> _isPlayerHidden = [
        new(OpCodes.Call,               // StartOfRound.Instance
                    AccessTools.Property(typeof(StartOfRound), nameof(StartOfRound.Instance)).GetMethod),
        new(OpCodes.Ldfld,              // .allPlayerScripts
            AccessTools.Field(typeof(StartOfRound), nameof(StartOfRound.allPlayerScripts))),
        new(OpCodes.Ldloc_2),           // index
        new(OpCodes.Ldelem_Ref),        // []
        new(OpCodes.Call,               // .IsHidden()
            AccessTools.Method(typeof(PlayerControllerBExtensions), nameof(PlayerControllerBExtensions.IsHidden)))
    ];

    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> HidePlayerFromPopCheck(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var matcher = new CodeMatcher(instructions);

        // Match for first isInsideFactory reference (part of jester close check)
        matcher.MatchForward(false, [new(OpCodes.Ldfld,
                AccessTools.Field(typeof(PlayerControllerB), nameof(PlayerControllerB.isInsideFactory)))]);

        // Get the continue branch target
        matcher.Advance(1);
        var continueLoopTarget = matcher.Instruction.operand;

        // Insert as separate && condition after this one
        matcher.Advance(1);
        // if (_isPlayerHidden) continue
        matcher.InsertAndAdvance(
            _isPlayerHidden
            .Union([
                new(OpCodes.Brtrue,    // continue
                    continueLoopTarget)
        ]));

        return matcher.InstructionEnumeration();
    }
}