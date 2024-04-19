using DramaMask.Config;
using DramaMask.Extensions;
using GameNetcodeStuff;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

namespace DramaMask.Patches.Enemies.RadMechAIPatch;

[HarmonyPatch(typeof(RadMechAI), nameof(RadMechAI.CheckSightForThreat))]
public class CheckSightForThreatPatch
{
    private static bool IsThreatHiddenPlayer(Collider collider)
    {
        return EnemyTargetHandler.ShouldHideFromEnemy(nameof(RadMechAI))
            && collider.transform.TryGetComponent<IVisibleThreat>(out var threat)
            && threat is PlayerControllerB player
            && player.IsHidden();
    }

    // Should only be called by the server so safe to use NetworkHandler.Instance.StealthMap
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> HidePlayerFromCollider(IEnumerable<CodeInstruction> instructions,
        ILGenerator generator)
    {
        var matcher = new CodeMatcher(instructions);

        // Match for the first branch, indicating the start of the loop
        matcher.MatchForward(false, [new(OpCodes.Br)]);

        // Match for the next reference to brtrue to get the continue target
        matcher.MatchForward(false, [new(OpCodes.Brtrue)]);
        var continueTarget = matcher.Instruction.operand;

        // Insert our continue condition after this one (cannot do before since jump to label would skip it)
        matcher.Advance(1);

        // if (RoundManager.Instance.tempColliderResults[index] is PlayerControllerB player && player.IsHidden()) continue;
        matcher.InsertAndAdvance([
            new(OpCodes.Call,           // RoundManager.Instance
                AccessTools.Property(typeof(RoundManager), nameof(RoundManager.Instance)).GetMethod),
            new(OpCodes.Ldfld,          // .tempColliderResults
                AccessTools.Field(typeof(RoundManager), nameof(RoundManager.tempColliderResults))),
            new(OpCodes.Ldloc_S, 5),    // index
            new(OpCodes.Ldelem_Ref),    // []
            new(OpCodes.Call,           // IsThreatHiddenPlayer()
                AccessTools.Method(typeof(CheckSightForThreatPatch), nameof(IsThreatHiddenPlayer))),
            new(OpCodes.Brtrue,         // continue
                continueTarget)
        ]);

        return matcher.InstructionEnumeration();
    }
}