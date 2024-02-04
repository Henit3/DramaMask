using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.IO;
using System.Reflection;
using UnityEngine;
using System.Linq;
using DramaMask.Extensions;
using BepInEx.Configuration;
using System;

namespace DramaMask;

[BepInDependency("evaisa.lethallib", "0.13.0")]
[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    public new static ManualLogSource Logger;

    private static readonly Harmony Harmony = new(PluginInfo.PLUGIN_GUID);

    private void Awake()
    {
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} ({PluginInfo.PLUGIN_VERSION}) is loading...");

        SetConfig();

        NetcodePatcher();

        Logger.LogInfo($"Loading assets...");
        LoadAssets();
        Logger.LogInfo($"Loading assets complete!");

        Logger.LogInfo($"Patching...");
        Harmony.PatchAll();
        Logger.LogInfo($"Patching complete!");

        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} ({PluginInfo.PLUGIN_VERSION}) is loaded!");
    }

    private static void NetcodePatcher()
    {
        var types = Assembly.GetExecutingAssembly().GetTypes();
        foreach (var type in types)
        {
            var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            foreach (var method in methods)
            {
                var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                if (attributes.Length > 0)
                {
                    method.Invoke(null, null);
                }
            }
        }
    }

    private void SetConfig()
    {
        var configSection = "Hiding Targets";

        ConfigValues.AllMasksHide = Config.Bind(
            new(configSection, "All Masks Hide"),
            ConfigValues.AllMasksHide,
            new("Whether all masks are able to hide the player from the Masked (if they don't get possessed first).",
                new AcceptableValueList<bool>(true, false))
        ).Value;

        ConfigValues.HideFromAllEnemies = Config.Bind(
            new(configSection, "Hide From All Enemies"),
            ConfigValues.HideFromAllEnemies,
            new("[EXPERIMENTAL] Whether the masks are able to hide the player from all types of enemies.",
                new AcceptableValueList<bool>(true, false))
        ).Value;


        configSection = "Stealth Meter";

        ConfigValues.UseStealthMeter = Config.Bind(
            new(configSection, "Use Stealth Meter"),
            ConfigValues.UseStealthMeter,
            new("Whether the masks should have a timeout limiting usage to make them balanced.",
                new AcceptableValueList<bool>(true, false))
        ).Value;

        ConfigValues.MaxHiddenTime = Config.Bind(
            new(configSection, "Max Stealth Time"),
            ConfigValues.MaxHiddenTime,
            new("How long players should be able to stay hidden with the mask activated from full.",
                new AcceptableValueRange<float>(0, 60))
        ).Value;

        ConfigValues.RechargeDelay = Config.Bind(
            new(configSection, "Stealth Recharge Delay"),
            ConfigValues.RechargeDelay,
            new("How long to wait until the stealth meter starts automatically recharging (countermeasure for spamming its activation).",
                new AcceptableValueRange<float>(0, 30))
        ).Value;

        ConfigValues.ExhaustionPenaltyDelay = Config.Bind(
            new(configSection, "Exhaustion Penalty Delay"),
            ConfigValues.ExhaustionPenaltyDelay,
            new("How long of a delay should be added as a penalty for fully exhausting the stealth meter.",
                new AcceptableValueRange<float>(0, 30))
        ).Value;


        configSection = "Stealth Meter HUD";

        ConfigValues.SeeStealthMeter = Config.Bind(
            new(configSection, "See Stealth Meter"),
            ConfigValues.SeeStealthMeter,
            new("Whether to show the stealth meter used when holding one of the masks that let you hide.",
                new AcceptableValueList<bool>(true, false))
        ).Value;

        ConfigValues.AlwaysSeeStealthMeter = Config.Bind(
            new(configSection, "Always see Stealth Meter"),
            ConfigValues.AlwaysSeeStealthMeter,
            new("Whether to always show the stealth meter, even when not holding a mask that lets you hide.",
                new AcceptableValueList<bool>(true, false))
        ).Value;

        ConfigValues.BarXPosition = Config.Bind(
            new(configSection, "Bar X Position"),
            ConfigValues.BarXPosition,
            new("The X position (horizontal) that the stealth bar will appear at on the screen (0 is the centre).",
                new AcceptableValueRange<float>(-380, 380))
        ).Value;

        ConfigValues.BarYPosition = Config.Bind(
            new(configSection, "Bar Y Position"),
            ConfigValues.BarYPosition,
            new("The Y position (vertical) that the stealth bar will appear at on the screen (0 is the centre).",
                new AcceptableValueRange<float>(-250, 250))
        ).Value;

        ConfigValues.BarColour = Config.Bind(
            new(configSection, "Bar Colour"),
            ConfigValues.BarColour.AsConfigString(),
            new("The colour that the stealth bar will appear as (format as \"r|g|b\" in hex, e.g. \"00|80|ff\" for cyan).")
        ).Value.FromConfigString();
    }

    private void LoadAssets()
    {
        var bundle = LoadBundle(PluginInfo.PLUGIN_GUID);
        if (bundle == null) return;

        Item dramaMask = bundle.SafeLoadAsset<Item>("assets/drama/dramamask.asset");
        if (dramaMask == null) return;

        LethalLib.Modules.Utilities.FixMixerGroups(dramaMask.spawnPrefab);
        LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(dramaMask.spawnPrefab);
        RegisterDramaScrap(dramaMask);
        Logger.LogDebug("Loaded asset: dramaMask");
    }

    /* Rarities decided with datasheet: https://docs.google.com/spreadsheets/d/1AREkZnHaqxukdpVNOEDFKikar9R4XAIjpZ_gI7NNngM/edit#gid=0
     * Kept between Tragedy and Comedy rarity values, on the lower side due to utility
     */
    private void RegisterDramaScrap(Item dramaMask)
    {
        LethalLib.Modules.Items.RegisterScrap(dramaMask, 3,
            LethalLib.Modules.Levels.LevelTypes.AssuranceLevel
        );
        LethalLib.Modules.Items.RegisterScrap(dramaMask, 40,
            LethalLib.Modules.Levels.LevelTypes.RendLevel
            | LethalLib.Modules.Levels.LevelTypes.DineLevel
            | LethalLib.Modules.Levels.LevelTypes.TitanLevel
            | LethalLib.Modules.Levels.LevelTypes.Modded
        );
    }

    private AssetBundle LoadBundle(string guid)
    {
        AssetBundle bundle = null;

        var assembly = Assembly.GetExecutingAssembly();
        var embeddedResources = assembly
            .GetManifestResourceNames();
        var bundleName = embeddedResources
            .FirstOrDefault(n => n.StartsWith($"{guid}.Assets"));
        if (bundleName == null)
        {
            Logger.LogError($"Embedded resource for [{guid}] not found!" +
                $"\nAvailable: [{string.Join(", ", embeddedResources)}]");
            return bundle;
        }

        try
        {
            Logger.LogDebug($"Loading embedded resource data [{bundleName}]...");
            using var str = assembly.GetManifestResourceStream(bundleName);
            using var memoryStream = new MemoryStream();
            str.CopyTo(memoryStream);

            bundle = AssetBundle.LoadFromMemory(memoryStream.ToArray());
        }
        catch (Exception e)
        {
            Logger.LogError($"Bundle [{bundleName}] failed to load!" +
                $"\nAvailable: [{string.Join(", ", bundle.GetAllAssetNames())}]" +
                $"\n{e}");
            return bundle;
        }

        Logger.LogDebug($"Bundle [{bundleName}] accessible!");
        return bundle;
    }
}
