using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace DramaMask.Patches.Enemies.ForestGiantAIPatch;

[HarmonyPatch(typeof(ForestGiantAI), nameof(ForestGiantAI.OnCollideWithPlayer))]
public class OnCollideWithPlayerPatch : OnCollideWithPlayerPatchBase
{
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> CheckIfNoCollide(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var matcher = new CodeMatcher(instructions);

        // Match for first stloc.0 (player assignment)
        matcher.MatchForward(false, [new(OpCodes.Stloc_0)]);

        AddNoCollideCheck(matcher, generator, new(OpCodes.Ldloc_0), nameof(ForestGiantAI));

        return matcher.InstructionEnumeration();
    }
}