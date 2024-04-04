using BepInEx.Configuration;
using CSync.Extensions;
using CSync.Lib;
using DramaMask.Extensions;
using DramaMask.Models;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using static LethalLib.Modules.Levels;

namespace DramaMask;
public class ConfigValues : SyncedConfig<ConfigValues>
{
    [DataMember] public SyncedEntry<bool> AllMasksHide;
    [DataMember] public SyncedEntry<bool> HideFromAllEnemies;

    /* Base rarities decided with datasheet: https://docs.google.com/spreadsheets/d/1AREkZnHaqxukdpVNOEDFKikar9R4XAIjpZ_gI7NNngM/edit#gid=0
     * Kept between Tragedy and Comedy rarity values, on the lower side due to utility
     */
    [DataMember] public SyncedEntry<int> BaseDramaSpawnChance;
    [DataMember] public SyncedEntry<string> CustomDramaSpawnConfig;
    public Dictionary<LevelTypes, int> DramaSpawnMapVanilla;
    public Dictionary<string, int> DramaSpawnMapModded;

    [DataMember] public SyncedEntry<bool> UseStealthMeter;
    [DataMember] public SyncedEntry<float> MaxHiddenTime;
    [DataMember] public SyncedEntry<float> ExhaustionPenaltyDelay;
    [DataMember] public SyncedEntry<float> RechargeDelay;
    [DataMember] public SyncedEntry<float> AttachedStealthMultiplier;
    [DataMember] public SyncedEntry<bool> RemoveOnDepletion;

    [DataMember] public SyncedEntry<bool> SeeStealthMeter;
    [DataMember] public SyncedEntry<bool> AlwaysSeeStealthMeter;
    [DataMember] public SyncedEntry<float> BarXPosition;
    [DataMember] public SyncedEntry<float> BarYPosition;
    [DataMember] public SyncedEntry<string> BarColourConfig;
    public Color BarColour;

    [DataMember] public SyncedEntry<string> HeldMaskView;
    [DataMember] public SyncedEntry<string> AttachedMaskView;

    public ConfigValues(ConfigFile cfg) : base(PluginInfo.PLUGIN_GUID)
    {
        ConfigManager.Register(this);
        InitialSyncCompleted += OnInitialSyncCompleted;

        SetHidingTargets(cfg);
        SetMaskSpawning(cfg);
        SetStealthMeter(cfg);
        SetStealthMeterHUD(cfg);
        SetMaskView(cfg);

        PostSyncProcessing();
    }

    private void SetHidingTargets(ConfigFile cfg)
    {
        const string section = "Hiding Targets";

        AllMasksHide = cfg.BindSyncedEntry(
            new(section, "All Masks Hide"),
            true,
            new ConfigDescription(
                "Whether all masks are able to hide the player from the Masked (if they don't get possessed first).",
                new AcceptableValueList<bool>(true, false)
            ));

        HideFromAllEnemies = cfg.BindSyncedEntry(
            new(section, "Hide From All Enemies"),
            false,
            new ConfigDescription(
                "[EXPERIMENTAL] Whether the masks are able to hide the player from all types of enemies.",
                new AcceptableValueList<bool>(true, false)
            ));
    }

    private void SetMaskSpawning(ConfigFile cfg)
    {
        const string section = "Mask Spawning";

        BaseDramaSpawnChance = cfg.BindSyncedEntry(
            new(section, "Base Drama Mask Spawn Chance"),
            40,
            new ConfigDescription(
                "The default spawn chance of drama masks",
                new AcceptableValueRange<int>(0, 1000)
            ));

        CustomDramaSpawnConfig = cfg.BindSyncedEntry(
            new(section, "Drama Mask Moon Spawn Chances"),
            new Dictionary<string, int>()
            {
                { "AssuranceLevel", 3 },
                { "VowLevel", 0 }
            }.AsString(),
            new ConfigDescription("Custom spawn chances for moons the Drama mask can spawn on, comma separated")
        );
    }

    private void SetStealthMeter(ConfigFile cfg)
    {
        const string section = "Stealth Meter";

        UseStealthMeter = cfg.BindSyncedEntry(
            new(section, "Use Stealth Meter"),
                    true,
            new ConfigDescription(
                "Whether the masks should have a timeout limiting usage to make them balanced.",
                new AcceptableValueList<bool>(true, false)
            ));

        MaxHiddenTime = cfg.BindSyncedEntry(
            new(section, "Max Stealth Time"),
            15f,
            new ConfigDescription(
                "How long players should be able to stay hidden with the mask activated from full.",
                new AcceptableValueRange<float>(0, 60)
            ));

        RechargeDelay = cfg.BindSyncedEntry(
            new(section, "Stealth Recharge Delay"),
            3f,
            new ConfigDescription(
                "How long to wait until the stealth meter starts automatically recharging (countermeasure for spamming its activation).",
                new AcceptableValueRange<float>(0, 30)
            ));

        ExhaustionPenaltyDelay = cfg.BindSyncedEntry(
            new(section, "Exhaustion Penalty Delay"),
            3f,
            new ConfigDescription(
                "How long of a delay should be added as a penalty for fully exhausting the stealth meter.",
                new AcceptableValueRange<float>(0, 30)
            ));

        AttachedStealthMultiplier = cfg.BindSyncedEntry(
            new(section, "Attached Stealth Multiplier"),
            1f,
            new ConfigDescription(
                "A multiplier to be applied to the stealth value when a mask is attached instead of held up.",
                new AcceptableValueRange<float>(0.1f, 10)
            ));

        RemoveOnDepletion = cfg.BindSyncedEntry(
            new(section, "Remove On Depletion"),
            false,
            new ConfigDescription("Whether the mask should take itself off when the stealth meter has been depleted.",
                new AcceptableValueList<bool>(true, false)
            ));
    }

    private void SetStealthMeterHUD(ConfigFile cfg)
    {
        const string section = "Stealth Meter HUD";

        SeeStealthMeter = cfg.BindSyncedEntry(
            new(section, "See Stealth Meter"),
            true,
            new ConfigDescription(
                "Whether to show the stealth meter used when holding one of the masks that let you hide.",
                new AcceptableValueList<bool>(true, false)
            ));

        AlwaysSeeStealthMeter = cfg.BindSyncedEntry(
            new(section, "Always see Stealth Meter"),
            false,
            new ConfigDescription(
                "Whether to always show the stealth meter, even when not holding a mask that lets you hide.",
                new AcceptableValueList<bool>(true, false)
            ));

        BarXPosition = cfg.BindSyncedEntry(
            new(section, "Bar X Position"),
            0f,
            new ConfigDescription(
                "The X position (horizontal) that the stealth bar will appear at on the screen (0 is the centre).",
                new AcceptableValueRange<float>(-380, 380)
            ));

        BarYPosition = cfg.BindSyncedEntry(
            new(section, "Bar Y Position"),
            235f,
            new ConfigDescription(
                "The Y position (vertical) that the stealth bar will appear at on the screen (0 is the centre).",
                new AcceptableValueRange<float>(-250, 250)
            ));

        BarColourConfig = cfg.BindSyncedEntry(
            new(section, "Bar Colour"),
            new Color(220, 220, 220, byte.MaxValue).AsConfigString(),
            new ConfigDescription("The colour that the stealth bar will appear as (format as \"r|g|b\" in hex, e.g. \"00|80|ff\" for cyan).")
        );
    }

    private void SetMaskView(ConfigFile cfg)
    {
        const string section = "Mask View";

        HeldMaskView = cfg.BindSyncedEntry(
            new(section, "Held Mask View"),
            MaskView.Opaque,
            new ConfigDescription(
                "How the mask appears when holding up a mask to your face.",
                new AcceptableValueList<string>(MaskView.Opaque/*, MaskView.Translucent*/, MaskView.Outline)
            ));

        AttachedMaskView = cfg.BindSyncedEntry(
            new(section, "Attached Mask View"),
            MaskView.MatchHeld,
            new ConfigDescription(
                "How the mask appears when attaching a mask to your face.",
                new AcceptableValueList<string>(MaskView.MatchHeld, MaskView.Opaque/*, MaskView.Translucent*/, MaskView.Outline)
            ));
    }

    private void OnInitialSyncCompleted(object s, EventArgs e) => PostSyncProcessing();
    private void PostSyncProcessing()
    {
        // Mask Spawning
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

        foreach (var pair in CustomDramaSpawnConfig.Value.Split(','))
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


        // Stealth Meter HUD
        BarColour = BarColourConfig.LocalValue.FromConfigString();


        // Mask View
        if (AttachedMaskView.Value is MaskView.MatchHeld) AttachedMaskView = HeldMaskView;
    }
}