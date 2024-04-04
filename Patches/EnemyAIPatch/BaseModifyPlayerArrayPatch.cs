using DramaMask.Extensions;
using DramaMask.Network;
using GameNetcodeStuff;
using System.Linq;

namespace DramaMask.Patches.EnemyAIPatch;

public abstract class BaseModifyPlayerArrayPatch
{
    private static PlayerControllerB[] allPlayerScriptsOriginal;

    protected static void SaveAndModifyPlayerArray(EnemyAI __instance)
    {
        if (!ShouldHideFromEnemy(__instance)) return;

        // Save old value and filter out searching for players that are activating a configured mask
        allPlayerScriptsOriginal = StartOfRound.Instance.allPlayerScripts;

        StartOfRound.Instance.allPlayerScripts = StartOfRound.Instance.allPlayerScripts
            .Where(player => player == null || !player.isPlayerControlled
                || NetworkHandler.Instance.VisiblePlayers.Contains(player.GetId()))
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