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
        matcher.MatchBack(false, [new(OpCodes.Add)]);
        matcher.Advance(-2);
        var continueLoopTarget1 = generator.DefineLabel();
        matcher.AddLabelsAt(matcher.Pos, [continueLoopTarget1]);

        // Skip this one again
        matcher.MatchForward(false, [new(OpCodes.Blt)]);
        matcher.Advance(1);

        // Match for next blt (player loop predicate 2)
        matcher.MatchForward(false, [new(OpCodes.Blt)]);
        matcher.MatchBack(false, [new(OpCodes.Add)]);
        matcher.Advance(-2);
        var continueLoopTarget2 = generator.DefineLabel();
        matcher.AddLabelsAt(matcher.Pos, [continueLoopTarget2]);

        // Restart search to find start of these loops
        matcher.Start();

        // Match for first bt (player loop start 1)
        matcher.MatchForward(false, [new(OpCodes.Br)]);

        // Add OOB check
        matcher.Advance(1);
        matcher.InsertAndAdvance([
            new(OpCodes.Ldloc_S, 5),        // index
            new(OpCodes.Call,               // IsOutOfBounds()
                AccessTools.Method(typeof(ModifyPlayerArrayPatch), nameof(IsOutOfBounds))),
            new(OpCodes.Brtrue,             // continue
                continueLoopTarget1)
        ]);

        // Match for next bt (player loop start 2)
        matcher.MatchForward(false, [new(OpCodes.Br)]);
        
        // Add OOB check
        matcher.Advance(1);
        matcher.InsertAndAdvance([
            new(OpCodes.Ldloc_S, 6),        // index
            new(OpCodes.Call,               // IsOutOfBounds()
                AccessTools.Method(typeof(ModifyPlayerArrayPatch), nameof(IsOutOfBounds))),
            new(OpCodes.Brtrue,             // continue
                continueLoopTarget2)
        ]);

        return matcher.InstructionEnumeration();
    }

    [HarmonyPostfix]
    public static void Postfix(DressGirlAI __instance)
    {
        LoadOriginalPlayerArray(__instance);
    }
}