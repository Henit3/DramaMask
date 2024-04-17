using DramaMask.Extensions;
using DramaMask.Patches.Base;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace DramaMask.Patches.ButlerEnemyAIPatch;

[HarmonyPatch(typeof(ButlerEnemyAI), nameof(ButlerEnemyAI.CheckLOS))]
public class CheckLOSPatch : ModifyPlayerArrayPatch
{
    [HarmonyPrefix]
    public static void Prefix(ButlerEnemyAI __instance)
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

        // Match for last blt (main loop predicate)
        matcher.End();
        matcher.MatchBack(false, [new(OpCodes.Blt)]);

        AddOobCheckToLoopPredicate(matcher, new(OpCodes.Ldloc_S, 4));

        // Replace all dereferencing using this index on other arrays by the playerClientId instead
        // This should be valid since the Nutcracker makes use of this as an index (lastPlayerSeenMoving)

        // Match for the recently replaced clt (main loop predicate) to go backward from
        matcher.MatchBack(false, [new(OpCodes.Clt)]);

        // Skip the first two instance of index (index < length, index++)
        matcher.MatchBack(false, [new(OpCodes.Ldloc_S)]);
        matcher.Advance(-1);
        matcher.MatchBack(false, [new(OpCodes.Ldloc_S)]);
        matcher.Advance(-1);

        while (true)
        {
            // Try find references to the index
            // Note: Will cause issues if more than one variable making use of ldloc.s in the future
            matcher.MatchBack(false, [new(OpCodes.Ldloc_S)]);

            // Break if no more instances to replace found
            if (matcher.Pos is -1) break;

            // Skip if replacing a direct assignment; specific to this code fragment
            matcher.Advance(1);
            if (matcher.Instruction.opcode == OpCodes.Stloc_3
                || matcher.Instruction.opcode == OpCodes.Stloc_S)
            {
                matcher.Advance(-2);
                continue;
            }

            // Skip replacing dereferences to StartOfRound.allPlayerScripts; self-referential
            matcher.Advance(-2);
            if (matcher.Instruction
                .Is(OpCodes.Ldfld,          // .allPlayerScripts
                    AccessTools.Field(typeof(StartOfRound), nameof(StartOfRound.allPlayerScripts))))
            {
                continue;
            }

            // Obtain the true player index (usable outside the modified array) using the loop index
            matcher.Advance(2);
            matcher.InsertAndAdvance([
                new(OpCodes.Call,           // .GetTrueIndex()
                    AccessTools.Method(typeof(ModifyPlayerArrayPatch), nameof(GetTrueIndex)))
            ]);
            matcher.Advance(-3);
        }

        return matcher.InstructionEnumeration();
    }

    [HarmonyPostfix]
    public static void Postfix(ButlerEnemyAI __instance)
    {
        LoadOriginalPlayerArray(__instance);
    }
}