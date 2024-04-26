using DramaMask.Config;
using DramaMask.Extensions;
using HarmonyLib;
using System.Reflection.Emit;

namespace DramaMask.Patches.Enemies;

public abstract class OnCollideWithPlayerPatchBase
{
    protected static void AddNoCollideCheck(CodeMatcher matcher, ILGenerator generator,
        CodeInstruction player, string enemyName)
    {
        matcher.Advance(1);

        // Label next instruction as the target to continue execution at
        var continueTarget = generator.DefineLabel();
        matcher.AddLabelsAt(matcher.Pos, [continueTarget]);

        // Add check for visbility and collision validity
        // if (player.IsHidden() && !EnemyTargetHandler.ShouldCollideWithEnemy(enemyName)) return (don't do it)
        matcher.InsertAndAdvance([
            player,                         // player
            new(OpCodes.Call,               // .IsHidden()
                AccessTools.Method(typeof(PlayerControllerBExtensions), nameof(PlayerControllerBExtensions.IsHidden))),
            new(OpCodes.Ldstr,              // enemyName
                enemyName),
            new(OpCodes.Call,               // EnemyTargetHandler.ShouldCollideWithEnemy(string _)
                AccessTools.Method(typeof(EnemyTargetHandler), nameof(EnemyTargetHandler.ShouldCollideWithEnemy),
                    [typeof(string)])),
            new(OpCodes.Not),               // !
            new(OpCodes.And),               // &&
            new(OpCodes.Brfalse,            // [continue to next instruction]
                continueTarget),
            new(OpCodes.Ret)                // return
        ]);
    }
}