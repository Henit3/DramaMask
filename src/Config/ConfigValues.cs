using BepInEx.Configuration;
using CSync.Extensions;
using CSync.Lib;
using DramaMask.Extensions;
using DramaMask.Constants;
using DramaMask.UI;
using LethalConfig;
using LethalConfig.ConfigItems;
using LethalConfig.ConfigItems.Options;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using static LethalLib.Modules.Levels;

namespace DramaMask.Config;
public class ConfigValues : SyncedConfig2<ConfigValues>
{
    // Entries not synced where they are only used server-side, or are local config settings

    [DataMember] public SyncedEntry<bool> AllMasksHide;
    public SyncedEntry<EnemyHideTargets> EnemiesHiddenFrom;
    public SyncedEntry<string> EnemyHidingOverrideConfig;
    public SyncedEntry<EnemyCollideTargets> EnemiesNoCollideOn;
    public SyncedEntry<float> MinCollideTime;
    public SyncedEntry<bool> IncreaseCustomEnemyCompatibility;
    [DataMember] public SyncedEntry<bool> AttachedCanPossess;

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
    [DataMember] public SyncedEntry<MeterVisibility> StealthMeterVisibility;
    private ConfigEntry<float> _meterOffset;
    public float MeterOffset;
    private ConfigEntry<bool> _accurateMeter;
    public bool AccurateMeter;
    private ConfigEntry<string> _meterColour;
    public Color MeterColour;

    [DataMember] public SyncedEntry<bool> SyncMaskView;
    /// <summary>
    /// Use LocalValue (syncing optional)
    /// </summary>
    [DataMember] public SyncedEntry<HeldMaskView> HeldMaskView;
    /// <summary>
    /// Use LocalValue (syncing optional)
    /// </summary>
    [DataMember] public SyncedEntry<AttachedMaskView> AttachedMaskViewConfig;
    private ConfigEntry<bool> _changeClientViewInstantly;
    public bool ChangeClientViewInstantly;

    private ConfigEntry<bool> _ignoreCustomKeybinds;
    public bool IgnoreCustomKeybinds;

    public ConfigValues(ConfigFile cfg) : base(PluginInfo.PLUGIN_GUID)
    {
        ConfigManager.Register(this);
        InitialSyncCompleted += (_, _) => PostSyncProcessing();

        SetHidingTargets(cfg);
        SetMaskSpawning(cfg);
        SetStealthMeter(cfg);
        SetStealthMeterHUD(cfg);
        SetMaskView(cfg);
        SetMisc(cfg);

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

        EnemiesHiddenFrom = cfg.BindSyncedEntry(
            new(section, "Enemies Hidden From"),
            EnemyHideTargets.Natural,
            new ConfigDescription(
                "The selection of enemies that attaching a mask hides you from (Natural attempts to stay true to the monster's usual behaviour)."
            ));

        EnemyHidingOverrideConfig = cfg.BindSyncedEntry(
            new(section, "Enemy Hiding Overrides"),
            new Dictionary<string, bool>()
            {
                { nameof(HoarderBugAI), false },
                { nameof(ForestGiantAI), true }
            }.AsString(),
            new ConfigDescription("Overrides for which enemies can be hidden from (exclusions prioritised), comma separated."
        ));
        EnemyHidingOverrideConfig.Changed += (_, _) => ProcessSyncEnemyHidingOverrideConfig();

        EnemiesNoCollideOn = cfg.BindSyncedEntry(
            new(section, "Enemies Without Collision Event"),
            EnemyCollideTargets.None,
            new ConfigDescription(
                "The selection of enemies that attaching a mask stops collision events/damage from."
            ));

        MinCollideTime = cfg.BindSyncedEntry(
            new(section, "Min Hiding Time to start ignoring Collisions"),
            3f,
            new ConfigDescription(
                "How long players are required to continuously hide before collision events are ignored.",
                new AcceptableValueRange<float>(0, 30)
            ));

        IncreaseCustomEnemyCompatibility = cfg.BindSyncedEntry(
            new(section, "Increased Custom Enemy Compatibility"),
            false,
            new ConfigDescription(
                "[Warning] Turning this on may allow support for some custom enemies, but could make the game run slower.",
                new AcceptableValueList<bool>(true, false)
            ));

        AttachedCanPossess = cfg.BindSyncedEntry(
            new(section, "Attached Masks Can Possess"),
            true,
            new ConfigDescription(
                "Whether masks that can possess you when held up, retain this behaviour when they are attached.",
                new AcceptableValueList<bool>(true, false)
            ));
    }

    private void SetMaskSpawning(ConfigFile cfg)
    {
        const string section = "Mask Spawning";

        BaseDramaSpawnChance = (_baseDramaSpawnChance = cfg.Bind(
            new(section, "Base Drama Mask Spawn Chance"),
            40,
            new ConfigDescription(
                "A multiplier applied to the default spawn chance of Drama masks (see mod description for details).",
                new AcceptableValueRange<int>(0, 1000)
            ))).Value;

        CustomDramaSpawnConfig = (_customDramaSpawnConfig = cfg.Bind(
            new(section, "Drama Mask Moon Spawn Chances"),
            new Dictionary<string, int>()
            {
                { "AssuranceLevel", 3 },
                { "VowLevel", 0 }
            }.AsString(),
            new ConfigDescription("Custom spawn chances for moons the Drama mask can spawn on, comma separated (Special values include: All, Vanilla, Modded).")
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

            var name = values[0].Trim();
            if (Enum.TryParse<LevelTypes>(name, true, out var levelType))
            {
                switch (levelType)
                {
                    case LevelTypes.All:
                        DramaSpawnMapVanilla.Clear();
                        DramaSpawnMapModded.Clear();
                        break;
                    case LevelTypes.Vanilla:
                        DramaSpawnMapVanilla.Clear();
                        break;
                    case LevelTypes.Modded:
                        DramaSpawnMapModded.Clear();
                        break;
                }
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
        SyncStealthMeterVisibility.Changed += (_, _) => ProcessSyncStealthMeterVisibility();

        StealthMeterVisibility = cfg.BindSyncedEntry(
            new(section, "Stealth Meter Visibility"),
            MeterVisibility.OnHold,
            new ConfigDescription(
                "When to show the stealth meter, if at all. [Optionally synced]"
            ));

        MeterOffset = (_meterOffset = cfg.Bind(
            new(section, "Meter Offset"),
            0.5f,
            new ConfigDescription(
                "The combined offset applied to the position of the stealth meter ring.",
                new AcceptableValueRange<float>(-3, 3)
            ))).Value;
        _meterOffset.SettingChanged += (_, _) =>
        {
            MeterOffset = _meterOffset.Value;
            StealthMeter.Instance.ApplyMeterOffsets();
        };

        AccurateMeter = (_accurateMeter = cfg.Bind(
            new(section, "Accurate Meter"),
            true,
            new ConfigDescription(
                "Whether the stealth meter ring should be accurate or have vanilla behaviour.",
                new AcceptableValueList<bool>(true, false)
            ))).Value;
        _accurateMeter.SettingChanged += (_, _) => AccurateMeter = _accurateMeter.Value;

        MeterColour = (_meterColour = cfg.Bind(
            new(section, "Bar Colour"),
            new Color(220, 220, 220, byte.MaxValue).AsConfigString(),
            new ConfigDescription("The colour that the stealth bar will appear as (format as \"r|g|b\" in hex, e.g. \"00|80|ff\" for cyan).")
        )).Value.FromConfigString();
        _meterColour.SettingChanged += (_, _) =>
        {
            MeterColour = _meterColour.Value.FromConfigString();
            StealthMeter.Instance.Colour = MeterColour;
        };
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
        SyncMaskView.Changed += (_, _) => ProcessSyncMaskView();

        HeldMaskView = cfg.BindSyncedEntry(
            new(section, "Held Mask View"),
            Constants.HeldMaskView.Opaque,
            new ConfigDescription(
                 "How the mask appears when holding up a mask to your face. [Optionally synced]"
            ));

        AttachedMaskViewConfig = cfg.BindSyncedEntry(
            new(section, "Attached Mask View"),
            Constants.AttachedMaskView.Translucent,
            new ConfigDescription(
                "How the mask appears when attaching a mask to your face. [Optionally synced]"
            ));

        ChangeClientViewInstantly = (_changeClientViewInstantly = cfg.Bind(
            new(section, "Instant Client Local Mask Actions"),
            true,
            new ConfigDescription(
                "Instant changes locally; can cause temporary visual desync on rapid changes.",
                new AcceptableValueList<bool>(true, false)
            ))).Value;
        _changeClientViewInstantly.SettingChanged += (_, _)
            => ChangeClientViewInstantly = _changeClientViewInstantly.Value;
    }

    private void SetMisc(ConfigFile cfg)
    {
        const string section = "Misc";

        IgnoreCustomKeybinds = (_ignoreCustomKeybinds = cfg.Bind(
            new(section, "Ignore Custom Keybinds"),
            false,
            new ConfigDescription(
                "If InputUtils installed, whether to ignore any custom keybinds; temporary compatibility for unconventional control schemes like with LCVR.",
                new AcceptableValueList<bool>(true, false)
            ))).Value;
        _ignoreCustomKeybinds.SettingChanged += (_, _)
            => IgnoreCustomKeybinds = _ignoreCustomKeybinds.Value;
    }

    private void OnInitialSyncCompleted(object s, EventArgs e) => ProcessSyncEnemyHidingOverrideConfig();
    private void PostSyncProcessing()
    {
        ProcessSyncEnemyHidingOverrideConfig();

        ProcessSyncStealthMeterVisibility();

        ProcessSyncMaskView();
    }

    private void ProcessSyncEnemyHidingOverrideConfig()
    {
        EnemyTargetHandler.OverrideInclusions.Clear();
        EnemyTargetHandler.OverrideExclusions.Clear();

        if (EnemyHidingOverrideConfig.Value is null) return;

        foreach (var pair in EnemyHidingOverrideConfig.Value.Split(','))
        {
            var values = pair.Split(':');
            if (values.Length != 2) continue;
            if (!bool.TryParse(values[1], out var toHideFrom)) continue;

            var name = values[0].Trim();
            if (toHideFrom)
            {
                EnemyTargetHandler.OverrideInclusions.Add(name);
                Plugin.Logger.LogInfo($"Registered enemy [{name}] to be hidden from");
            }
            else
            {
                EnemyTargetHandler.OverrideExclusions.Add(name);
                Plugin.Logger.LogInfo($"Registered enemy [{name}] to not be hidden from");
            }
        }
    }

    private void ProcessSyncStealthMeterVisibility()
    {
        if (!SyncStealthMeterVisibility.Value) return;

        StealthMeterVisibility.LocalValue = StealthMeterVisibility.Value;
    }

    private void ProcessSyncMaskView()
    {
        if (SyncMaskView.Value)
        {
            HeldMaskView.LocalValue = HeldMaskView.Value;
            AttachedMaskViewConfig.LocalValue = AttachedMaskViewConfig.Value;
        }
        if (AttachedMaskViewConfig.LocalValue is AttachedMaskView.MatchHeld)
        {
            AttachedMaskViewConfig.LocalValue = (AttachedMaskView)(int)HeldMaskView.LocalValue;
        }
    }

    private void SetupLethalConfig()
    {
        LethalConfigManager.AddConfigItem(new BoolCheckBoxConfigItem(AllMasksHide.Entry, requiresRestart: false));
        LethalConfigManager.AddConfigItem(new EnumDropDownConfigItem<EnemyHideTargets>(EnemiesHiddenFrom.Entry, requiresRestart: false));
        LethalConfigManager.AddConfigItem(new TextInputFieldConfigItem(EnemyHidingOverrideConfig.Entry, new TextInputFieldOptions()
        {
            NumberOfLines = 1,
            TrimText = true,
            RequiresRestart = false
        }));
        LethalConfigManager.AddConfigItem(new EnumDropDownConfigItem<EnemyCollideTargets>(EnemiesNoCollideOn.Entry, requiresRestart: false));
        LethalConfigManager.AddConfigItem(new FloatStepSliderConfigItem(MinCollideTime.Entry, new FloatStepSliderOptions()
        {
            Min = 0,
            Max = 30,
            Step = 0.1f,
            RequiresRestart = false
        }));
        LethalConfigManager.AddConfigItem(new BoolCheckBoxConfigItem(IncreaseCustomEnemyCompatibility.Entry, requiresRestart: true));
        LethalConfigManager.AddConfigItem(new BoolCheckBoxConfigItem(AttachedCanPossess.Entry, requiresRestart: false));

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
        LethalConfigManager.AddConfigItem(new EnumDropDownConfigItem<MeterVisibility>(StealthMeterVisibility.Entry, requiresRestart: false));
        LethalConfigManager.AddConfigItem(new FloatStepSliderConfigItem(_meterOffset, new FloatStepSliderOptions()
        {
            Min = -3f,
            Max = 3f,
            Step = 0.1f,
            RequiresRestart = false
        }));
        LethalConfigManager.AddConfigItem(new TextInputFieldConfigItem(_meterColour, new TextInputFieldOptions()
        {
            NumberOfLines = 1,
            TrimText = true,
            RequiresRestart = false
        }));

        LethalConfigManager.AddConfigItem(new BoolCheckBoxConfigItem(SyncMaskView.Entry, requiresRestart: false));
        LethalConfigManager.AddConfigItem(new EnumDropDownConfigItem<AttachedMaskView>(AttachedMaskViewConfig.Entry, requiresRestart: false));
        LethalConfigManager.AddConfigItem(new EnumDropDownConfigItem<HeldMaskView>(HeldMaskView.Entry, requiresRestart: false));
        LethalConfigManager.AddConfigItem(new BoolCheckBoxConfigItem(_changeClientViewInstantly, requiresRestart: false));

        LethalConfigManager.AddConfigItem(new BoolCheckBoxConfigItem(_ignoreCustomKeybinds, requiresRestart: false));
    }

    private static string GetDescriptionWithOptions(string description, string[] options)
        => $"{description}\nOptions: [{string.Join(", ", options)}]";
}