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
using BepInEx.Bootstrap;
using LethalLib.Modules;
using DramaMask.Models;

namespace DramaMask;

[BepInDependency("evaisa.lethallib", "0.15.0")]
[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    public new static ManualLogSource Logger;

    public static readonly bool IsMoreEmotesPresent = Chainloader.PluginInfos.ContainsKey("MoreEmotes-Sligili");

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
        
        configSection = "Mask Spawning";

        ConfigValues.BaseDramaSpawnChance = Config.Bind(
            new(configSection, "Base Drama Mask Spawn Chance"),
            ConfigValues.BaseDramaSpawnChance,
            new("The default spawn chance of drama masks",
                new AcceptableValueRange<int>(0, 1000))
        ).Value;

        ConfigValues.CustomDramaSpawnConfig = Config.Bind(
            new(configSection, "Drama Mask Moon Spawn Chances"),
            ConfigValues.CustomDramaSpawnConfig,
            new("Custom spawn chances for moons the Drama mask can spawn on, comma separated")
        ).Value;
        ConfigValues.ParseRarityConfigString();

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

        ConfigValues.AttachedStealthMultiplier = Config.Bind(
            new(configSection, "Attached Stealth Multiplier"),
            ConfigValues.AttachedStealthMultiplier,
            new("A multiplier to be applied to the stealth value when a mask is attached instead of held up.",
                new AcceptableValueRange<float>(0.1f, 10))
        ).Value;

        ConfigValues.RemoveOnDepletion = Config.Bind(
            new(configSection, "Remove On Depletion"),
            ConfigValues.RemoveOnDepletion,
            new("Whether the mask should take itself off when the stealth meter has been depleted.",
                new AcceptableValueList<bool>(true, false))
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


        configSection = "Miscellaneous";

        ConfigValues.SelectedMaskView = Config.Bind(
            new(configSection, "Mask View"),
            ConfigValues.SelectedMaskView,
            new("Whether to only see the mask outline when wearing a mask.",
                new AcceptableValueList<string>(MaskView.Opaque/*, MaskView.Translucent*/, MaskView.Outline))
        ).Value;
    }

    private void LoadAssets()
    {
        var maskBundle = LoadBundle(PluginInfo.PLUGIN_GUID, "dramamask");
        if (maskBundle == null) return;

        var dramaMask = maskBundle.SafeLoadAsset<Item>("assets/drama/dramamask.asset");
        if (dramaMask == null) return;

        Utilities.FixMixerGroups(dramaMask.spawnPrefab);
        NetworkPrefabs.RegisterNetworkPrefab(dramaMask.spawnPrefab);
        Items.RegisterScrap(dramaMask,
             ConfigValues.DramaSpawnMapVanilla,
             ConfigValues.DramaSpawnMapModded);
        Logger.LogDebug("Loaded asset: dramaMask");

        var outlineBundle = LoadBundle(PluginInfo.PLUGIN_GUID, "maskoutline");
        if (outlineBundle == null) return;

        var outlineMesh = outlineBundle.SafeLoadAsset<Mesh>("assets/outline/maskoutline.001.mesh");
        if (outlineMesh == null) return;

        HauntedMaskItemExtensions.OutlineMesh = outlineMesh;
        Logger.LogDebug("Loaded asset: maskOutline");

        /*var armsOutBundle = LoadBundle(PluginInfo.PLUGIN_GUID, "armsout");
        if (armsOutBundle == null) return;

        var armsOutAnimation = armsOutBundle.SafeLoadAsset<AnimationClip>("assets/custom/armsout.anim");
        if (armsOutAnimation == null) return;

        HauntedMaskItemExtensions.ArmsOutAnimation = armsOutAnimation;
        Logger.LogDebug("Loaded asset: armsOut");*/
    }

    
    private void RegisterDramaScrap(Item dramaMask)
    {
        Items.RegisterScrap(dramaMask,
            ConfigValues.DramaSpawnMapVanilla,
            ConfigValues.DramaSpawnMapModded);
    }

    private AssetBundle LoadBundle(string guid, string bundleName)
    {
        AssetBundle bundle = null;

        var assembly = Assembly.GetExecutingAssembly();
        var embeddedResources = assembly
            .GetManifestResourceNames();
        var resourceName = embeddedResources
            .FirstOrDefault(n => n.StartsWith($"{guid}.Assets.{bundleName}"));
        if (resourceName == null)
        {
            Logger.LogError($"Embedded resource for [{guid}] not found!" +
                $"\nAvailable: [{string.Join(", ", embeddedResources)}]");
            return bundle;
        }

        try
        {
            Logger.LogDebug($"Loading embedded resource data [{resourceName}]...");
            using var str = assembly.GetManifestResourceStream(resourceName);
            using var memoryStream = new MemoryStream();
            str.CopyTo(memoryStream);

            bundle = AssetBundle.LoadFromMemory(memoryStream.ToArray());
        }
        catch (Exception e)
        {
            Logger.LogError($"Bundle [{resourceName}] failed to load!" +
                $"\nAvailable: [{string.Join(", ", bundle.GetAllAssetNames())}]" +
                $"\n{e}");
            return bundle;
        }

        Logger.LogDebug($"Bundle [{resourceName}] accessible!");
        return bundle;
    }
}
