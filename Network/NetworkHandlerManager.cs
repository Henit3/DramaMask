using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace DramaMask.Network;

[HarmonyPatch]
public class NetworkHandlerManager
{
    private static GameObject _networkPrefab;

    [HarmonyPostfix, HarmonyPatch(typeof(GameNetworkManager), "Start")]
    public static void Init()
    {
        if (_networkPrefab != null)
            return;

        _networkPrefab = LethalLib.Modules.NetworkPrefabs.CreateNetworkPrefab(PluginInfo.PLUGIN_GUID);
        _networkPrefab.AddComponent<NetworkHandler>();

        NetworkManager.Singleton.AddNetworkPrefab(_networkPrefab);
    }

    [HarmonyPostfix, HarmonyPatch(typeof(StartOfRound), "Awake")]
    static void SpawnNetworkHandler()
    {
        // Only the host should spawn network objects
        if (!NetworkHandler.IsHostOrServer()) return;

        var networkHandlerHost = Object.Instantiate(_networkPrefab, Vector3.zero, Quaternion.identity);
        networkHandlerHost.GetComponent<NetworkObject>().Spawn();
    }
}