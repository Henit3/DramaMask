using DramaMask.Extensions;
using System;
using System.Collections.Generic;
using UnityEngine;
using static LethalLib.Modules.Levels;

namespace DramaMask;

public static class ConfigValues
{
    public static bool AllMasksHide = true;
    public static bool HideFromAllEnemies = false;

    /* Base rarities decided with datasheet: https://docs.google.com/spreadsheets/d/1AREkZnHaqxukdpVNOEDFKikar9R4XAIjpZ_gI7NNngM/edit#gid=0
     * Kept between Tragedy and Comedy rarity values, on the lower side due to utility
     */
    public static int BaseDramaSpawnChance = 40;
    public static string CustomDramaSpawnConfig = new Dictionary<string, int>()
    {
        { "AssuranceLevel", 3 },
        { "VowLevel", 0 }
    }.AsString();
    public static Dictionary<LevelTypes, int> DramaSpawnMapVanilla;
    public static Dictionary<string, int> DramaSpawnMapModded;

    public static bool UseStealthMeter = true;
    public static float ExhaustionPenaltyDelay = 3;
    public static float MaxHiddenTime = 15;
    public static float RechargeDelay = 3;

    public static float AttachedStealthMultiplier = 1f;

    public static bool SeeStealthMeter = true;
    public static bool AlwaysSeeStealthMeter = false;
    public static float BarXPosition = 0f;
    public static float BarYPosition = 235f;
    public static Color BarColour = new(220, 220, 220, byte.MaxValue);

    public static bool SeeWornMaskOutline = false;

    public static void ParseRarityConfigString()
    {
        DramaSpawnMapVanilla = new()
        {
            { LevelTypes.AssuranceLevel, (int)Math.Round(BaseDramaSpawnChance*(3/40f)) },
            { LevelTypes.RendLevel, BaseDramaSpawnChance },
            { LevelTypes.DineLevel, BaseDramaSpawnChance },
            { LevelTypes.TitanLevel, BaseDramaSpawnChance },
            { LevelTypes.Modded, BaseDramaSpawnChance }
        };
        DramaSpawnMapModded = new();

        if (CustomDramaSpawnConfig is null) return;

        foreach (var pair in CustomDramaSpawnConfig.Split(','))
        {
            var values = pair.Split(':');
            if (values.Length != 2) continue;
            if (!int.TryParse(values[1], out var spawnrate)) continue;

            var name = values[0];
            if (Enum.TryParse<LevelTypes>(name, true, out var levelType))
            {
                DramaSpawnMapVanilla[levelType] = spawnrate;
                Plugin.Logger.LogDebug($"Registered spawn rate for level type {levelType} to {spawnrate}");
            }
            else
            {
                DramaSpawnMapModded[name] = spawnrate;
                Plugin.Logger.LogDebug($"Registered spawn rate for modded level {name} to {spawnrate}");
            }
        }
    }
}
