using System.Collections.Generic;

namespace DramaMask.Constants;

public static class EnemyTargets
{
    public const string Masked = "Masked";
    public const string Natural = "Natural";
    public const string All = "All";

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

        if (Plugin.Config.EnemiesHiddenFrom.Value is All) return true;
        if (Plugin.Config.EnemiesHiddenFrom.Value is Masked) return enemyName is nameof(MaskedPlayerEnemy);

        return !NaturalExceptions.Contains(enemyName);
    }
}
