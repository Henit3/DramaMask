using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace DramaMask.Patches.Enemies.EnemyAIPatch;

[HarmonyPatch(typeof(EnemyAI), nameof(EnemyAI.TargetClosestPlayer))]
public class TargetClosestPlayerPatch : ModifyPlayerArrayPatchBase
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
    private static IEnumerable<CodeInstruction> StopOutOfBounds(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var matcher = new CodeMatcher(instructions);

        // Go to end and match backwards for first blt (main player loop predicate)
        matcher.End();
        matcher.MatchBack(false, [new(OpCodes.Blt)]);

        AddOobCheckToLoopPredicate(matcher, new(OpCodes.Ldloc_1));

        return matcher.InstructionEnumeration();
    }

    [HarmonyPostfix]
    public static void Postfix(EnemyAI __instance, ref bool __result,
        float bufferDistance = 1.5f, bool requireLineOfSight = false, float viewWidth = 70f)
    {
        LoadOriginalPlayerArray(__instance);
    }
}