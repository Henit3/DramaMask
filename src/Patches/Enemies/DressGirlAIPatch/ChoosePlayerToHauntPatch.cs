using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace DramaMask.Patches.Enemies.DressGirlAIPatch;

[HarmonyPatch(typeof(DressGirlAI), "ChoosePlayerToHaunt")]
public class ChoosePlayerToHauntPatch : ModifyPlayerArrayPatchBase
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

        AddOobCheckToLoopPredicate(matcher, new(OpCodes.Ldloc_S, 5));

        // Repeat for second player loop
        matcher.MatchForward(false, [new(OpCodes.Blt)]);

        AddOobCheckToLoopPredicate(matcher, new(OpCodes.Ldloc_S, 6));

        return matcher.InstructionEnumeration();
    }

    [HarmonyPostfix]
    public static void Postfix(DressGirlAI __instance)
    {
        LoadOriginalPlayerArray(__instance);
    }
}