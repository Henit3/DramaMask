using DramaMask.Extensions;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace DramaMask.Patches.RedLocustBeesPatch;

[HarmonyPatch(typeof(RedLocustBees), nameof(RedLocustBees.DoAIInterval))]
public class DoAIIntervalPatch
{
    // Do not check if the player is hidden
    private static readonly List<CodeInstruction> _isPlayerHidden = [
        new(OpCodes.Call,               // StartOfRound.Instance
                AccessTools.Property(typeof(StartOfRound), nameof(StartOfRound.Instance)).GetMethod),
        new(OpCodes.Ldfld,              // .localPlayerController
            AccessTools.Field(typeof(StartOfRound), nameof(StartOfRound.localPlayerController))),
        new(OpCodes.Call,               // .IsHidden()
            AccessTools.Method(typeof(PlayerControllerBExtensions), nameof(PlayerControllerBExtensions.IsHidden)))
    ];

    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> HidePlayerFromColliderPatch(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var matcher = new CodeMatcher(instructions);

        // Match for first array dereference (part of player assignment)
        matcher.MatchForward(false, [new(OpCodes.Ldelem_Ref)]);

        // Skip to the end of the assignment, and end of the predicate
        matcher.Advance(7);

        // Skip the current player from being targeted
        // && !_isPlayerHidden
        matcher.InsertAndAdvance(
            _isPlayerHidden
            .Union([
                new(OpCodes.Not),           // !
                new(OpCodes.And)            // &
        ]));

        return matcher.InstructionEnumeration();
    }
}