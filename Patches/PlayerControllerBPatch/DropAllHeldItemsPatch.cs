using DramaMask.Extensions;
using DramaMask.Network;
using GameNetcodeStuff;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Unity.Netcode;
using UnityEngine;

namespace DramaMask.Patches.PlayerControllerBPatch;

[HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.DropAllHeldItems))]
public class DropAllHeldItemsPatch
{
    private static readonly List<CodeInstruction> _isMaskAttached = [
        new(OpCodes.Ldarg_0),           // this
        new(OpCodes.Call,               // DropAllHeldItemsPatch.IsMaskAttached
            AccessTools.Method(typeof(DropAllHeldItemsPatch), nameof(DropAllHeldItemsPatch.IsMaskAttached)))
    ];
    private static bool IsMaskAttached(PlayerControllerB player)
    {
        var mask = player.currentlyHeldObjectServer as HauntedMaskItem;
        return mask != null && mask.currentHeadMask != null;
    }

    // Do not check if the player is dropping on death or disconnection
    private static readonly List<CodeInstruction> _isPlayerValid = [
        new(OpCodes.Ldarg_0),           // this
        new(OpCodes.Ldarg_2),           // disconnecting
        new(OpCodes.Call,               // DropAllHeldItemsPatch.IsMaskAttached
            AccessTools.Method(typeof(DropAllHeldItemsPatch), nameof(DropAllHeldItemsPatch.IsPlayerValid)))
    ];
    private static bool IsPlayerValid(PlayerControllerB player, bool disconnecting)
    {
        return !player.isPlayerDead && !disconnecting;
    }

    // Check if current item is held out
    // itemSlot == this.CurrentlyHeldObjectServer
    private static readonly List<CodeInstruction> _currentItemIsHeld = [
        new(OpCodes.Ldloc_0),           // itemSlot
        new(OpCodes.Ldarg_0),           // this
        new(OpCodes.Ldfld,              // .CurrentlyHeldObjectServer
            AccessTools.Field(typeof(PlayerControllerB), nameof(PlayerControllerB.currentlyHeldObjectServer))),
        new(OpCodes.Ceq)                // ==
    ];

    [HarmonyPrefix]
    public static void Prefix(PlayerControllerB __instance, bool itemsFall = true, bool disconnecting = false)
    {
        if (IsPlayerValid(__instance, disconnecting)) return;

        var mask = __instance.currentlyHeldObjectServer as HauntedMaskItem;

        // Reset maskAttached status (will also remove currentHeadMask to stop mask duplication)
        var targetPretendData = __instance.IsLocal()
            ? NetworkHandler.Instance.MyPretend
            : NetworkHandler.Instance.PretendMap[__instance.GetId()];
        targetPretendData.IsMaskAttached = false;

        // Stop headmask persisting after player is dead (host disconnect: SaveItemsInShipPatch)
        if (__instance.isPlayerDead && mask != null && mask.currentHeadMask != null)
        {
            Object.Destroy(mask.currentHeadMask.gameObject);
            mask.currentHeadMask = null;
        }
    }

    [HarmonyPostfix]
    public static void Postfix(PlayerControllerB __instance, bool itemsFall = true, bool disconnecting = false)
    {
        if (!IsPlayerValid(__instance, disconnecting)) return;

        // Reset weight to the correct value if mask is attached (done outside transpilation for simplicity)
        if (IsMaskAttached(__instance))
        {
            __instance.carryWeight = __instance.currentlyHeldObjectServer.itemProperties.weight;
        }
    }

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
        // if (_isPlayerValid && (_isMaskAttached && _currentItemIsHeld)) continue;
        matcher.InsertAndAdvance(
            _isPlayerValid
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
        // && !(_isPlayerValid && _isMaskAttached)
        matcher.InsertAndAdvance(
            _isPlayerValid
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
        // if (_isPlayerValid && _isMaskAttached) return;
        matcher.InsertAndAdvance(
            _isPlayerValid
            .Union(_isMaskAttached)
            .Union([
                new(OpCodes.And),           // &&
                new(OpCodes.Brtrue,         // return
                    returnTarget)
        ]));

        return matcher.InstructionEnumeration();
    }
}
