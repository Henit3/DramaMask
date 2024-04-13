using DramaMask.Extensions;
using GameNetcodeStuff;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

namespace DramaMask.Patches.CrawlerAIPatch;

[HarmonyPatch(typeof(CrawlerAI), "CheckForVeryClosePlayer")]
public class CheckForVeryClosePlayerPatch
{
    // Return first visible player collider
    private static readonly List<CodeInstruction> _getVisiblePlayerCollider = [
        //new(OpCodes.Ldarg_0),         // this (use pre-existing one since it has a label attached)
        new(OpCodes.Ldfld,              // .nearPlayerColliders
            AccessTools.Field(typeof(CrawlerAI), "nearPlayerColliders")),
        new(OpCodes.Call,               // GetVisiblePlayerCollider()
            AccessTools.Method(typeof(CheckForVeryClosePlayerPatch), nameof(GetVisiblePlayerCollider)))
    ];
    private static PlayerControllerB GetVisiblePlayerCollider(Collider[] nearPlayerColliders)
    {
        foreach (var collider in nearPlayerColliders)
        {
            var player = collider.transform.GetComponent<PlayerControllerB>();
            if (player == null || player.IsHidden()) continue;
            return player;
        }
        return null;
    }

    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> HidePlayerFromCollider(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var matcher = new CodeMatcher(instructions);

        // Match for first ret (after player check)
        matcher.MatchForward(false, [new(OpCodes.Ret)]);

        // Remove code to store the first player collider
        matcher.Advance(2);
        matcher.RemoveInstructions(5);

        // Get the first player that is visible to store instead
        matcher.InsertAndAdvance(_getVisiblePlayerCollider);

        return matcher.InstructionEnumeration();
    }
}