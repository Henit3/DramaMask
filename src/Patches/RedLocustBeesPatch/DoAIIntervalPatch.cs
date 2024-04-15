using DramaMask.Constants;
using DramaMask.Extensions;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace DramaMask.Patches.RedLocustBeesPatch;

[HarmonyPatch(typeof(RedLocustBees), nameof(RedLocustBees.DoAIInterval))]
public class DoAIIntervalPatch
{
    private static bool IsPlayerHidden()
    {
        return EnemyTargets.ShouldHideFromEnemy(nameof(RedLocustBees))
            && StartOfRound.Instance.localPlayerController.IsHidden();
    }

    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> HidePlayerFromCollider(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var matcher = new CodeMatcher(instructions);

        // Match for first array dereference (part of player assignment)
        matcher.MatchForward(false, [new(OpCodes.Ldelem_Ref)]);

        // Skip to the end of the assignment, and end of the predicate
        matcher.Advance(7);

        // Skip the current player from being targeted
        // && !IsPlayerHidden()
        matcher.InsertAndAdvance(
            new(OpCodes.Call,           // IsPlayerHidden()
                AccessTools.Method(typeof(DoAIIntervalPatch), nameof(IsPlayerHidden))),
            new(OpCodes.Not),           // !
            new(OpCodes.And)            // &
        );

        return matcher.InstructionEnumeration();
    }
}