using DramaMask.Patches.Base;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace DramaMask.Patches.DressGirlAIPatch;

[HarmonyPatch(typeof(DressGirlAI), "ChoosePlayerToHaunt")]
public class ChoosePlayerToHauntPatch : ModifyPlayerArrayPatch
{
    [HarmonyPrefix]
    public static void Prefix(DressGirlAI __instance)
    {
        SaveAndModifyPlayerArray(__instance);
    }

    // Out of bounds handling with static player count transpilation from other mods
    [HarmonyAfter([
        "notnotnotswipez-MoreCompany-1.8.1",
        "bizzlemip-BiggerLobby-2.6.0",
        "PotatoePet-AdvancedCompany-1.0.150"
    ])]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> StopOutOfBounds(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var matcher = new CodeMatcher(instructions);

        // Match for first blt (player loop predicate 1)
        matcher.MatchForward(false, [new(OpCodes.Blt)]);

        // Store the branch target to use in adjusted predicate
        var enterLoopTarget1 = matcher.Instruction.operand;

        // Replace blt with clt to only check as part of first "and" condition
        matcher.RemoveInstruction().InsertAndAdvance([new(OpCodes.Clt)]);

        // Add second "and" condition to check if out of bounds, branching into processing loop if both satisfied
        // ... && IsWithinPlayerBounds(index)
        matcher.InsertAndAdvance(
            new(OpCodes.Ldloc_S, 5),        // index
            new(OpCodes.Call,               // IsWithinPlayerBounds()
                AccessTools.Method(typeof(ModifyPlayerArrayPatch), nameof(IsWithinPlayerBounds))),
            new(OpCodes.And),               // &&
            new(OpCodes.Brtrue,
                enterLoopTarget1)            // enter processing loop
        );

        // Repeat for second player loop
        matcher.MatchForward(false, [new(OpCodes.Blt)]);
        var enterLoopTarget2 = matcher.Instruction.operand;
        matcher.RemoveInstruction().InsertAndAdvance([new(OpCodes.Clt)]);

        // ... && IsWithinPlayerBounds(index)
        matcher.InsertAndAdvance(
            new(OpCodes.Ldloc_S, 6),        // index
            new(OpCodes.Call,               // IsWithinPlayerBounds()
                AccessTools.Method(typeof(ModifyPlayerArrayPatch), nameof(IsWithinPlayerBounds))),
            new(OpCodes.And),               // &&
            new(OpCodes.Brtrue,
                enterLoopTarget1)            // enter processing loop
        );

        return matcher.InstructionEnumeration();
    }

    [HarmonyPostfix]
    public static void Postfix(DressGirlAI __instance)
    {
        LoadOriginalPlayerArray(__instance);
    }
}