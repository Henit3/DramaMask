using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.IO;
using System.Reflection;
using UnityEngine;
using System.Linq;
using DramaMask.Extensions;
using System;
using LethalLib.Modules;

namespace DramaMask;

[BepInDependency("evaisa.lethallib", "0.15.0")]
[BepInDependency("com.sigurd.csync", "4.1.0")]
[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    public new static ManualLogSource Logger;
    internal static new ConfigValues Config;

    //public static readonly bool IsMoreEmotesPresent = Chainloader.PluginInfos.ContainsKey("MoreEmotes-Sligili");

    private static readonly Harmony Harmony = new(PluginInfo.PLUGIN_GUID);

    private void Awake()
    {
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} ({PluginInfo.PLUGIN_VERSION}) is loading...");

        Config = new ConfigValues(base.Config);

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

    private void LoadAssets()
    {
        var maskBundle = LoadBundle(PluginInfo.PLUGIN_GUID, "dramamask");
        if (maskBundle == null) return;

        var dramaMask = maskBundle.SafeLoadAsset<Item>("assets/drama/dramamask.asset");
        if (dramaMask == null) return;

        Utilities.FixMixerGroups(dramaMask.spawnPrefab);
        NetworkPrefabs.RegisterNetworkPrefab(dramaMask.spawnPrefab);
        Items.RegisterScrap(dramaMask,
             Config.DramaSpawnMapVanilla,
             Config.DramaSpawnMapModded);
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
