﻿using GameNetcodeStuff;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace DramaMask.Patches.Enemies.EnemyAIPatch;

[HarmonyPatch(typeof(EnemyAI), nameof(EnemyAI.CheckLineOfSightForClosestPlayer))]
public class CheckLineOfSightForClosestPlayerPatch : ModifyPlayerArrayPatchBase
{
    [HarmonyPrefix]
    public static void Prefix(EnemyAI __instance,
        float width = 45f, int range = 60, int proximityAwareness = -1, float bufferDistance = 0f)
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

        AddOobCheckToLoopPredicate(matcher, generator, new(OpCodes.Ldloc_S, 4));

        return matcher.InstructionEnumeration();
    }

    [HarmonyPostfix]
    public static void Postfix(EnemyAI __instance, ref PlayerControllerB __result,
        float width = 45f, int range = 60, int proximityAwareness = -1, float bufferDistance = 0f)
    {
        LoadOriginalPlayerArray(__instance);
    }
}