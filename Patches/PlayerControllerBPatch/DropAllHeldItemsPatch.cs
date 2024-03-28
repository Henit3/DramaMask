using DramaMask.Extensions;
using DramaMask.Network;
using DramaMask.Network.Models;
using GameNetcodeStuff;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Unity.Netcode;

namespace DramaMask.Patches.PlayerControllerBPatch;

[HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.DropAllHeldItems))]
public class DropAllHeldItemsPatch : BaseChangeItemPatch
{
    // Gathered instructions like a macro for convenience
    // NetworkHandler.Instance.MyPretend.IsMaskAttached
    private static readonly List<CodeInstruction> _isMaskAttached = [
        new(OpCodes.Call,               // NetworkHandler.Instance
            AccessTools.Property(typeof(NetworkHandler), nameof(NetworkHandler.Instance)).GetMethod),
        new(OpCodes.Call,               // .MyPretend
            AccessTools.Property(typeof(NetworkHandler), nameof(NetworkHandler.MyPretend)).GetMethod),
        new(OpCodes.Call,               // .IsMaskAttached
            AccessTools.Property(typeof(PretendData), nameof(PretendData.IsMaskAttached)).GetMethod)
    ];

    // Do not check if the player is dropping on death or disconnection
    // !(this.isPlayerDead || disconnecting)
    private static readonly List<CodeInstruction> _shouldCheckMask = [
        new(OpCodes.Ldarg_0),           // this
        new(OpCodes.Ldfld,              // .isPlayerDead
            AccessTools.Field(typeof(PlayerControllerB), nameof(PlayerControllerB.isPlayerDead))),
        new(OpCodes.Ldarg_2),           // disconnecting
        new(OpCodes.Or),                // ||
        new(OpCodes.Not),               // !
    ];

    // Check if current item is held out
    // itemSlot == this.CurrentlyHeldObjectServer
    private static readonly List<CodeInstruction> _currentItemIsHeld = [
        new(OpCodes.Ldloc_0),           // itemSlot
        new(OpCodes.Ldarg_0),           // this
        new(OpCodes.Ldfld,              // .CurrentlyHeldObjectServer
            AccessTools.Field(typeof(PlayerControllerB), nameof(PlayerControllerB.currentlyHeldObjectServer))),
        new(OpCodes.Ceq)                // ==
    ];

    /*[HarmonyPrefix]
    public static void Prefix(PlayerControllerB __instance, bool itemsFall = true, bool disconnecting = false)
    {
        if (!disconnecting) return;

        // Do this on the server since the player who's doing this will disconnect anyway
        if (!NetworkHandler.IsHostOrServer()) return;

        // If player had mask attached, detach the mask before it is dropped
        var id = __instance.GetId();
        var targetData = NetworkHandler.Instance.PretendMap[id];
        if (targetData.IsMaskAttached) targetData.IsMaskAttached = false;
    }*/

    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> IgnoreDroppingAttachedMask(IEnumerable<CodeInstruction> instructions,
        ILGenerator generator)
    {
        var matcher = new CodeMatcher(instructions);

        // Find and store the continue target label
        // index++
        matcher.MatchForward(false, [
            new(OpCodes.Ldloc_1),           // index
            new(OpCodes.Ldc_I4_1),          // 1
            new(OpCodes.Add),               // +
            new(OpCodes.Stloc_1)            // index =
        ]);
        var continueTarget = generator.DefineLabel();
        matcher.AddLabelsAt(matcher.Pos, [continueTarget]);

        // Find and store the return target label
        matcher.MatchForward(false, [
            new(OpCodes.Ret)                // return
        ]);
        var returnTarget = generator.DefineLabel();
        matcher.AddLabelsAt(matcher.Pos, [returnTarget]);

        // Reset and match to go after the first instruction of the item slot loop
        // itemSlot = this.ItemSlots[index]
        matcher.Start();
        matcher.MatchForward(true, [
            new(OpCodes.Ldarg_0),           // this
            new(OpCodes.Ldfld,              // .ItemSlots
                AccessTools.Field(typeof(PlayerControllerB), nameof(PlayerControllerB.ItemSlots))),
            new(OpCodes.Ldloc_1),           // index
            new(OpCodes.Ldelem_Ref),        // []
            new(OpCodes.Stloc_0)            // itemSlot =
        ]);
        matcher.Advance(1);

        // Skip dropping the active mask if a mask is attached and it is currently being held
        // if (_shouldCheckMask && (_isMaskAttached && _currentItemIsHeld)) continue;
        matcher.InsertAndAdvance(
            _shouldCheckMask
            .Union(_isMaskAttached)
            .Union(_currentItemIsHeld)
            .Union([
                new(OpCodes.And),           // &&
                new(OpCodes.And),           // &&
                new(OpCodes.Brtrue,         // continue
                    continueTarget)
        ]));

        // Match for the if condition in the loop to discard item on client if the owner
        // if (this.isHoldingObject)
        matcher.MatchForward(true, [
            new(OpCodes.Ldarg_0),           // this
            new(OpCodes.Call,               // .IsOwner
                AccessTools.Property(typeof(NetworkBehaviour), nameof(NetworkBehaviour.IsOwner)).GetMethod)
        ]);
        matcher.Advance(1);

        // Add condition to stop discard item on client if mask is attached
        // && !(_shouldCheckMask && _isMaskAttached)
        matcher.InsertAndAdvance(
            _shouldCheckMask
            .Union(_isMaskAttached)
            .Union([
                new(OpCodes.And),           // &&
                new(OpCodes.Not),           // !
                new(OpCodes.And)            // &&
        ]));

        // Match for the if condition after the loop, and set branch inside if
        //      (don't want to change loop end target for maximum compatibility)
        // if (this.isHoldingObject)
        matcher.MatchForward(true, [
            new(OpCodes.Ldarg_0),           // this
            new(OpCodes.Ldfld,              // .isHoldingObject
                AccessTools.Field(typeof(PlayerControllerB), nameof(PlayerControllerB.isHoldingObject)))
        ]);
        matcher.Advance(2);

        // Skip overall resets if a mask is attached
        // if (_shouldCheckMask && _isMaskAttached) return;
        matcher.InsertAndAdvance(
            _shouldCheckMask
            .Union(_isMaskAttached)
            .Union([
                new(OpCodes.And),           // &&
                new(OpCodes.Brtrue,         // return
                    returnTarget)
        ]));

        Plugin.Logger.LogDebugInstructionsFrom(matcher);

        return matcher.InstructionEnumeration();
    }
}
