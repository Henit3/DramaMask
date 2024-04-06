﻿using BepInEx.Configuration;
using CSync.Extensions;
using CSync.Lib;
using DramaMask.Extensions;
using DramaMask.Models;
using LethalConfig;
using LethalConfig.ConfigItems;
using LethalConfig.ConfigItems.Options;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using static LethalLib.Modules.Levels;

namespace DramaMask.Config;
public class ConfigValues : SyncedConfig<ConfigValues>
{
    // Entries not synced where they are only used server-side, or are local config settings

    [DataMember] public SyncedEntry<bool> AllMasksHide;
    private ConfigEntry<bool> _hideFromAllEnemies;
    public bool HideFromAllEnemies;

    /* Base rarities decided with datasheet: https://docs.google.com/spreadsheets/d/1AREkZnHaqxukdpVNOEDFKikar9R4XAIjpZ_gI7NNngM/edit#gid=0
     * Kept between Tragedy and Comedy rarity values, on the lower side due to utility
     */
    private ConfigEntry<int> _baseDramaSpawnChance;
    public int BaseDramaSpawnChance;
    private ConfigEntry<string> _customDramaSpawnConfig;
    public string CustomDramaSpawnConfig;
    public Dictionary<LevelTypes, int> DramaSpawnMapVanilla;
    public Dictionary<string, int> DramaSpawnMapModded;

    [DataMember] public SyncedEntry<bool> UseStealthMeter;
    [DataMember] public SyncedEntry<float> MaxHiddenTime;
    [DataMember] public SyncedEntry<float> ExhaustionPenaltyDelay;
    [DataMember] public SyncedEntry<float> RechargeDelay;
    [DataMember] public SyncedEntry<float> AttachedStealthMultiplier;
    [DataMember] public SyncedEntry<bool> RemoveOnDepletion;

    [DataMember] public SyncedEntry<bool> SyncStealthMeterVisibility;
    /// <summary>
    /// Use LocalValue (syncing optional)
    /// </summary>
    [DataMember] public SyncedEntry<string> StealthMeterVisibility;
    private ConfigEntry<float> _barXPosition;
    public float BarXPosition;
    private ConfigEntry<float> _barYPosition;
    public float BarYPosition;
    private ConfigEntry<string> _barColour;
    public Color BarColour;

    [DataMember] public SyncedEntry<bool> SyncMaskView;
    /// <summary>
    /// Use LocalValue (syncing optional)
    /// </summary>
    [DataMember] public SyncedEntry<string> HeldMaskView;
    /// <summary>
    /// Use LocalValue (syncing optional)
    /// </summary>
    [DataMember] public SyncedEntry<string> AttachedMaskView;
    private ConfigEntry<bool> _changeClientViewInstantly;
    public bool ChangeClientViewInstantly;

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

        try { SetupLethalConfig(); }
        catch { Plugin.Logger.LogInfo("Soft dependency on LethalConfig could not be loaded."); }
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

        HideFromAllEnemies = (_hideFromAllEnemies = cfg.Bind(
            new(section, "Hide From All Enemies"),
            false,
            new ConfigDescription(
                "[EXPERIMENTAL] Whether the masks are able to hide the player from all types of enemies.",
                new AcceptableValueList<bool>(true, false)
            ))).Value;
    }

    private void SetMaskSpawning(ConfigFile cfg)
    {
        const string section = "Mask Spawning";

        BaseDramaSpawnChance = (_baseDramaSpawnChance = cfg.Bind(
            new(section, "Base Drama Mask Spawn Chance"),
            40,
            new ConfigDescription(
                "The default spawn chance of drama masks.",
                new AcceptableValueRange<int>(0, 1000)
            ))).Value;

        CustomDramaSpawnConfig = (_customDramaSpawnConfig = cfg.Bind(
            new(section, "Drama Mask Moon Spawn Chances"),
            new Dictionary<string, int>()
            {
                { "AssuranceLevel", 3 },
                { "VowLevel", 0 }
            }.AsString(),
            new ConfigDescription("Custom spawn chances for moons the Drama mask can spawn on, comma separated.")
        )).Value;


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

        SyncStealthMeterVisibility = cfg.BindSyncedEntry(
            new(section, "Sync Stealth Meter Visibility"),
            true,
            new ConfigDescription(
                "Whether to sync the 'Stealth Meter Visibility' config values.",
                new AcceptableValueList<bool>(true, false)
            ));

        StealthMeterVisibility = cfg.BindSyncedEntry(
            new(section, "Stealth Meter Visibility"),
            MeterVisibility.OnHold,
            new ConfigDescription(
                "When to show the stealth meter, if at all. [Optionally synced]",
                new AcceptableValueList<string>(MeterVisibility.Never, MeterVisibility.OnHold, MeterVisibility.Always)
            ));

        BarXPosition = (_barXPosition = cfg.Bind(
            new(section, "Bar X Position"),
            0f,
            new ConfigDescription(
                "The X position (horizontal) that the stealth bar will appear at on the screen (0 is the centre).",
                new AcceptableValueRange<float>(-380, 380)
            ))).Value;

        BarYPosition = (_barYPosition = cfg.Bind(
            new(section, "Bar Y Position"),
            235f,
            new ConfigDescription(
                "The Y position (vertical) that the stealth bar will appear at on the screen (0 is the centre).",
                new AcceptableValueRange<float>(-250, 250)
            ))).Value;

        BarColour = (_barColour = cfg.Bind(
            new(section, "Bar Colour"),
            new Color(220, 220, 220, byte.MaxValue).AsConfigString(),
            new ConfigDescription("The colour that the stealth bar will appear as (format as \"r|g|b\" in hex, e.g. \"00|80|ff\" for cyan).")
        )).Value.FromConfigString();
    }

    private void SetMaskView(ConfigFile cfg)
    {
        const string section = "Mask View";

        SyncMaskView = cfg.BindSyncedEntry(
            new(section, "Sync Mask View"),
            true,
            new ConfigDescription(
                "Whether to sync the Held & Attached 'Mask View' config values.",
                new AcceptableValueList<bool>(true, false)
            ));

        HeldMaskView = cfg.BindSyncedEntry(
            new(section, "Held Mask View"),
            MaskView.Opaque,
            new ConfigDescription(
                "How the mask appears when holding up a mask to your face. [Optionally synced]",
                new AcceptableValueList<string>(MaskView.Opaque, MaskView.Translucent, MaskView.Outline)
            ));

        AttachedMaskView = cfg.BindSyncedEntry(
            new(section, "Attached Mask View"),
            MaskView.Translucent,
            new ConfigDescription(
                "How the mask appears when attaching a mask to your face. [Optionally synced]",
                new AcceptableValueList<string>(MaskView.MatchHeld, MaskView.Opaque, MaskView.Translucent, MaskView.Outline)
            ));

        ChangeClientViewInstantly = (_changeClientViewInstantly = cfg.Bind(
            new(section, "Instant Client Local Mask Actions"),
            true,
            new ConfigDescription(
                "Instant changes locally; can cause temporary visual desync on rapid changes.",
                new AcceptableValueList<bool>(true, false)
            ))).Value;
    }

    private void OnInitialSyncCompleted(object s, EventArgs e) => PostSyncProcessing();
    private void PostSyncProcessing()
    {
        // Stealth Meter HUD
        if (SyncStealthMeterVisibility.Value)
        {
            StealthMeterVisibility.LocalValue = StealthMeterVisibility.Value;
        }

        // Mask View
        if (SyncMaskView.Value)
        {
            HeldMaskView.LocalValue = HeldMaskView.Value;
            AttachedMaskView.LocalValue = AttachedMaskView.Value;
        }
        if (AttachedMaskView.LocalValue is MaskView.MatchHeld)
        {
            AttachedMaskView.LocalValue = HeldMaskView.LocalValue;
        }
    }

    private void SetupLethalConfig()
    {
        // Would've liked a string option config item for the enum-like constants

        LethalConfigManager.AddConfigItem(new BoolCheckBoxConfigItem(AllMasksHide.Entry, requiresRestart: false));
        LethalConfigManager.AddConfigItem(new BoolCheckBoxConfigItem(_hideFromAllEnemies, requiresRestart: false));

        LethalConfigManager.AddConfigItem(new IntSliderConfigItem(_baseDramaSpawnChance, new IntSliderOptions()
        {
            Min = 0,
            Max = 1000,
            RequiresRestart = true
        }));
        LethalConfigManager.AddConfigItem(new TextInputFieldConfigItem(_customDramaSpawnConfig, new TextInputFieldOptions()
        {
            NumberOfLines = 1,
            TrimText = true,
            RequiresRestart = true
        }));

        LethalConfigManager.AddConfigItem(new BoolCheckBoxConfigItem(UseStealthMeter.Entry, requiresRestart: false));
        LethalConfigManager.AddConfigItem(new FloatStepSliderConfigItem(MaxHiddenTime.Entry, new FloatStepSliderOptions()
        {
            Min = 0,
            Max = 60,
            Step = 0.1f,
            RequiresRestart = false
        }));
        LethalConfigManager.AddConfigItem(new FloatStepSliderConfigItem(ExhaustionPenaltyDelay.Entry, new FloatStepSliderOptions()
        {
            Min = 0,
            Max = 30,
            Step = 0.1f,
            RequiresRestart = false
        }));
        LethalConfigManager.AddConfigItem(new FloatStepSliderConfigItem(RechargeDelay.Entry, new FloatStepSliderOptions()
        {
            Min = 0,
            Max = 30,
            Step = 0.1f,
            RequiresRestart = false
        }));
        LethalConfigManager.AddConfigItem(new FloatStepSliderConfigItem(AttachedStealthMultiplier.Entry, new FloatStepSliderOptions()
        {
            Min = 0.1f,
            Max = 10f,
            Step = 0.1f,
            RequiresRestart = false
        }));
        LethalConfigManager.AddConfigItem(new BoolCheckBoxConfigItem(RemoveOnDepletion.Entry, requiresRestart: false));

        LethalConfigManager.AddConfigItem(new BoolCheckBoxConfigItem(SyncStealthMeterVisibility.Entry, requiresRestart: false));
        LethalConfigManager.AddConfigItem(new TextInputFieldConfigItem(StealthMeterVisibility.Entry, new TextInputFieldOptions()
        {
            NumberOfLines = 1,
            TrimText = true,
            RequiresRestart = false
        }));
        LethalConfigManager.AddConfigItem(new FloatStepSliderConfigItem(_barXPosition, new FloatStepSliderOptions()
        {
            Min = -380f,
            Max = 380f,
            Step = 0.1f,
            RequiresRestart = false
        }));
        LethalConfigManager.AddConfigItem(new FloatStepSliderConfigItem(_barYPosition, new FloatStepSliderOptions()
        {
            Min = -250f,
            Max = 250f,
            Step = 0.1f,
            RequiresRestart = false
        }));
        LethalConfigManager.AddConfigItem(new TextInputFieldConfigItem(_barColour, new TextInputFieldOptions()
        {
            NumberOfLines = 1,
            TrimText = true,
            RequiresRestart = false
        }));

        LethalConfigManager.AddConfigItem(new BoolCheckBoxConfigItem(SyncMaskView.Entry, requiresRestart: false));
        LethalConfigManager.AddConfigItem(new TextInputFieldConfigItem(AttachedMaskView.Entry, new TextInputFieldOptions()
        {
            NumberOfLines = 1,
            TrimText = true,
            RequiresRestart = false
        }));
        LethalConfigManager.AddConfigItem(new TextInputFieldConfigItem(HeldMaskView.Entry, new TextInputFieldOptions()
        {
            NumberOfLines = 1,
            TrimText = true,
            RequiresRestart = false
        }));
        LethalConfigManager.AddConfigItem(new BoolCheckBoxConfigItem(_changeClientViewInstantly, requiresRestart: false));
    }
}