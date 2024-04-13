using DramaMask.Extensions;
using GameNetcodeStuff;
using System.Linq;

namespace DramaMask.Patches.Base;

public abstract class ModifyPlayerArrayPatch
{
    private static PlayerControllerB[] allPlayerScriptsOriginal;

    // For accompanying OOB checking transpilation
    protected static bool IsOutOfBounds(int index)
    {
        return index < StartOfRound.Instance.allPlayerScripts.Length;
    }

    protected static void SaveAndModifyPlayerArray(EnemyAI __instance)
    {
        if (!ShouldHideFromEnemy(__instance)) return;

        // Save old value and filter out searching for players that are activating a configured mask
        allPlayerScriptsOriginal = StartOfRound.Instance.allPlayerScripts;

        StartOfRound.Instance.allPlayerScripts = StartOfRound.Instance.allPlayerScripts
            .Where(player => player == null || !player.isPlayerControlled || player.IsHidden())
            .ToArray();
    }

    protected static void LoadOriginalPlayerArray(EnemyAI __instance)
    {
        if (!ShouldHideFromEnemy(__instance)) return;

        // Reset the player array to its old value
        StartOfRound.Instance.allPlayerScripts = allPlayerScriptsOriginal;
    }

    private static bool ShouldHideFromEnemy(EnemyAI __instance)
    {
        return Plugin.Config.HideFromAllEnemies
            || __instance is MaskedPlayerEnemy;
    }
}