using DramaMask.Patches.Base;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace DramaMask.Patches.RadMechAIPatch;

[HarmonyPatch(typeof(RadMechAI), "LookForPlayersInFlight")]
public class LookForPlayersInFlightPatch : ModifyPlayerArrayPatch
{
    [HarmonyPrefix]
    public static void Prefix(RadMechAI __instance)
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

        AddOobCheckToLoopPredicate(matcher, new(OpCodes.Ldloc_0));

        return matcher.InstructionEnumeration();
    }

    [HarmonyPostfix]
    public static void Postfix(RadMechAI __instance)
    {
        LoadOriginalPlayerArray(__instance);
    }
}