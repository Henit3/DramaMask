using DramaMask.Extensions;
using DramaMask.Network;
using GameNetcodeStuff;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace DramaMask.Patches.TurretPatch;

[HarmonyPatch(typeof(Turret), nameof(Turret.CheckForPlayersInLineOfSight))]
public class CheckForPlayersInLineOfSightPatch
{
    // Do not check if the player is hidden
    private static readonly List<CodeInstruction> _isPlayerHidden = [
        new(OpCodes.Ldloc_3),           // (PlayerControllerB) component
        new(OpCodes.Call,               // CheckForPlayersInLineOfSightPatch.IsPlayerHidden
            AccessTools.Method(typeof(CheckForPlayersInLineOfSightPatch), nameof(IsPlayerHidden)))
    ];
    private static bool IsPlayerHidden(PlayerControllerB player)
    {
        return NetworkHandler.Instance.VisiblePlayers != null
            && !NetworkHandler.Instance.VisiblePlayers.Contains(player.GetId());
    }

    // Should only be called by the server so safe to use NetworkHandler.Instance.StealthMap
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> IgnoreHiddenPlayersPatch(IEnumerable<CodeInstruction> instructions,
        ILGenerator generator)
    {
        var matcher = new CodeMatcher(instructions);

        // Go to end and match for "Player", where it stores any players found (main player loop predicate)
        matcher.MatchForward(false, [new(OpCodes.Ldstr, "Player")]);
        matcher.MatchForward(false, [new(OpCodes.Ldarg_0)]);

        // Go back since the br type is not agreed upon to match for (dotPeek: brfalse.s; harmony: brfalse)
        matcher.Advance(-1);

        // Store the branch target to use in adjusted predicate
        var skipPlayerTarget = matcher.Instruction.operand;

        // Skip to the player assignment so we can insert our extra condition after this
        matcher.MatchForward(true, [new(OpCodes.Stloc_3)]);
        matcher.Advance(1);

        // Skip activating the turret if the found player is hidden
        // if (_isPlayerHidden) <skip next if branch>;
        matcher.InsertAndAdvance(
            _isPlayerHidden
            .Union([
                new(OpCodes.Brtrue,         // <skip next if branch>
                    skipPlayerTarget)
        ]));

        return matcher.InstructionEnumeration();
    }
}