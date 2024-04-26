using DramaMask.Constants;
using System.Collections.Generic;

namespace DramaMask.Config;

public static class EnemyTargetHandler
{
    public static readonly HashSet<string> NaturalExceptions = [
        nameof(BaboonBirdAI),
        nameof(SandSpiderAI),
        nameof(HoarderBugAI),
        nameof(RedLocustBees),
        nameof(DressGirlAI),
        nameof(SandWormAI),
        nameof(RadMechAI),
        nameof(ButlerBeesEnemyAI)
    ];
    public static readonly HashSet<string> OverrideInclusions = [];
    public static readonly HashSet<string> OverrideExclusions = [];
    public static bool ShouldHideFromEnemy(EnemyAI enemy)
        => ShouldHideFromEnemy(enemy.GetType().Name);
    public static bool ShouldHideFromEnemy(string enemyName)
    {
        if (OverrideExclusions.Contains(enemyName)) return false;
        if (OverrideInclusions.Contains(enemyName)) return true;

        if (Plugin.Config.EnemiesHiddenFrom.Value is EnemyHideTargets.All) return true;
        if (Plugin.Config.EnemiesHiddenFrom.Value is EnemyHideTargets.Masked) return enemyName is nameof(MaskedPlayerEnemy);

        return !NaturalExceptions.Contains(enemyName);
    }

    public static bool ShouldCollideWithEnemy(EnemyAI enemy)
        => ShouldCollideWithEnemy(enemy.GetType().Name);
    public static bool ShouldCollideWithEnemy(string enemyName)
    {
        return Plugin.Config.EnemiesNoCollideOn.Value switch
        {
            EnemyCollideTargets.None => true,
            EnemyCollideTargets.Masked => enemyName is not nameof(MaskedPlayerEnemy),
            EnemyCollideTargets.Hidden => !ShouldHideFromEnemy(enemyName),
            _ => true
        };
    }
}

