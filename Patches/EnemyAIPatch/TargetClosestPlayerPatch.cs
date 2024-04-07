using GameNetcodeStuff;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace DramaMask.Patches.EnemyAIPatch;

[HarmonyPatch(typeof(EnemyAI), nameof(EnemyAI.TargetClosestPlayer))]
public class TargetClosestPlayerPatch : BaseModifyPlayerArrayPatch
{
    [HarmonyPrefix]
    public static void Prefix(EnemyAI __instance,
        float bufferDistance = 1.5f, bool requireLineOfSight = false, float viewWidth = 70f)
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
    private static IEnumerable<CodeInstruction> SetMaskTypePatch(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var matcher = new CodeMatcher(instructions);

        // Go to end and match backwards for first blt (main player loop predicate)
        matcher.End();
        matcher.MatchBack(false, [new(OpCodes.Blt)]);

        // Store the branch target to use in adjusted predicate
        var enterLoopTarget = matcher.Instruction.operand;

        // Replace blt with clt to only check as part of first "and" condition
        matcher.RemoveInstruction().InsertAndAdvance([new(OpCodes.Clt)]);

        // Add second "and" condition to check if out of bounds
        matcher.InsertAndAdvance(
            new(OpCodes.Ldloc_1),           // index

            new(OpCodes.Call,               // StartOfRound.Instance
                AccessTools.Property(typeof(StartOfRound), nameof(StartOfRound.Instance)).GetMethod),
            new(OpCodes.Ldfld,              // .allPlayerScripts
                AccessTools.Field(typeof(StartOfRound), nameof(StartOfRound.allPlayerScripts))),
            new(OpCodes.Ldlen),             // .Length
            new(OpCodes.Conv_I4),           // (int)

            new(OpCodes.Clt)                // <
        );

        // Apply "and" operation and branch into processing loop if both satisfied
        matcher.InsertAndAdvance(
            new(OpCodes.And),                // (index < checkLength) && (index in bounds)
            new(OpCodes.Brtrue,
                enterLoopTarget)             // enter processing loop
        );

        return matcher.InstructionEnumeration();
    }

    [HarmonyPostfix]
    public static void Postfix(EnemyAI __instance, ref bool __result,
        float bufferDistance = 1.5f, bool requireLineOfSight = false, float viewWidth = 70f)
    {
        LoadOriginalPlayerArray(__instance);
    }
}