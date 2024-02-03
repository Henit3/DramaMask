using GameNetcodeStuff;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace DramaMask.Patches.EnemyAIPatch;

[HarmonyPatch(typeof(EnemyAI), nameof(EnemyAI.GetClosestPlayer))]
public class GetClosestPlayerPatch : BaseModifyPlayerArrayPatch
{
    [HarmonyPrefix]
    public static void Prefix(EnemyAI __instance,
        bool requireLineOfSight = false, bool cannotBeInShip = false, bool cannotBeNearShip = false)
    {
        SaveAndModifyPlayerArray(__instance);
    }

    // Fixes the hardcoded player count
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> SetMaskTypePatch(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var matcher = new CodeMatcher(instructions);

        // index < 4
        CodeMatch[] forLoopPredicate = [
            new (OpCodes.Ldloc_1),             // index
            new (OpCodes.Ldc_I4_4),            // 4
            new (OpCodes.Blt)                  // <
        ];

        // Check and go to the hardcoded predicate if it still exists
        matcher.MatchForward(false, forLoopPredicate);

        // If match unsuccessful, dev has probably fixed this and transpilation is not needed
        if (matcher.Remaining == 0)
        {
            Plugin.Logger.LogInfo("Could not find [index < 4] predicate to patch.");
            return matcher.InstructionEnumeration();
        }

        // Jump to the position of the 4 constant to replace it
        matcher.Advance(1);

        // Replace 4 with StartOfRound.Instance.allPlayerScripts.Length
        matcher.RemoveInstruction()
            .InsertAndAdvance(
                new (OpCodes.Call,              // StartOfRound.Instance
                    typeof(StartOfRound).GetMethod("get_Instance", BindingFlags.Public | BindingFlags.Static)),
                new (OpCodes.Ldfld,             // .allPlayerScripts
                    typeof(StartOfRound).GetField("allPlayerScripts", BindingFlags.Public | BindingFlags.Instance)),
                new (OpCodes.Ldlen),            // .Length
                new (OpCodes.Conv_I4)           // (int)
            );

        return matcher.InstructionEnumeration();
    }

    [HarmonyPostfix]
    public static void Postfix(EnemyAI __instance, ref PlayerControllerB __result,
        bool requireLineOfSight = false, bool cannotBeInShip = false, bool cannotBeNearShip = false)
    {
        LoadOriginalPlayerArray(__instance);
    }
}