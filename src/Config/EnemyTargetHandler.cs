using DramaMask.Constants;
using DramaMask.Extensions;
using DramaMask.Network;
using GameNetcodeStuff;
using System;
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

    public static bool ShouldCollideWithEnemy(PlayerControllerB player, EnemyAI enemy)
        => ShouldCollideWithEnemy(player, enemy.GetType().Name);
    public static bool ShouldCollideWithEnemy(PlayerControllerB player, string enemyName)
    {
        var targetStealthData = player.IsLocal()
            ? NetworkHandler.Instance.MyStealth
            : NetworkHandler.Instance.StealthMap[player.GetId()];

        if (targetStealthData.LastStartedStealth.HasValue
            && DateTime.UtcNow.Subtract(targetStealthData.LastStartedStealth.Value)
                .TotalSeconds < Plugin.Config.MinCollideTime.Value)
        {
            return true;
        }

        return Plugin.Config.EnemiesNoCollideOn.Value switch
        {
            EnemyCollideTargets.None => true,
            EnemyCollideTargets.Masked => enemyName is not nameof(MaskedPlayerEnemy),
            EnemyCollideTargets.Hidden => !ShouldHideFromEnemy(enemyName),
            _ => true
        };
    }
}

