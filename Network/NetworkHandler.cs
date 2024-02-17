using DramaMask.Extensions;
using DramaMask.Network.Models;
using System.Collections.Generic;
using Unity.Netcode;

namespace DramaMask.Network;

public class NetworkHandler : NetworkBehaviour
{
    public static NetworkHandler Instance { get; private set; }

    public NetworkList<ulong> VisiblePlayers;
    
    public Dictionary<ulong, StealthData> StealthMap { get; private set; }
    public StealthData MyStealth { get; private set; }

    public Dictionary<ulong, PretendData> PretendMap { get; private set; }
    public PretendData MyPretend { get; private set; }

    public static bool IsHostOrServer() => NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer;

    private void Awake()
    {
        VisiblePlayers = new NetworkList<ulong>();
        // Need to initialise for most methods to work
        VisiblePlayers.Initialize(this);
    }

    public override void OnNetworkSpawn()
    {
        // Allows other GameObjects access to our instance
        var previousInstance = Instance;
        Instance = this;
        MyStealth = new();
        MyPretend = new();

        // Initialise network variables
        if (IsHostOrServer())
        {
            // Host always has id = 0
            StealthMap = new();
            PretendMap = new();
            VisiblePlayers.Clear();
            RegisterPlayer(0);
            
            // Register a method to be run on client connectivity callbacks
            NetworkManager.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
            NetworkManager.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectedCallback;
        }
        // Register actions on changing of the network variable
        VisiblePlayers.OnListChanged += OnVisiblePlayersChanged;

        // Continue normal behaviour and remove additional subscriptions duplicating this
        base.OnNetworkSpawn();
    }

    public override void OnNetworkDespawn()
    {
        if (IsHostOrServer())
        {
            NetworkManager.OnClientConnectedCallback -= NetworkManager_OnClientConnectedCallback;
            NetworkManager.OnClientDisconnectCallback -= NetworkManager_OnClientDisconnectedCallback;
        }

        // Unregister actions on changing of the network variable before disposal
        VisiblePlayers.OnListChanged -= OnVisiblePlayersChanged;

        base.OnNetworkDespawn();
    }

    private void NetworkManager_OnClientConnectedCallback(ulong obj)
    {
        // Skip if the host connected
        if (obj == 0) return;
        RegisterPlayer(obj);
    }
    private void RegisterPlayer(ulong id)
    {
        if (!StealthMap.ContainsKey(id)) StealthMap.Add(id, new(id));
        if (!PretendMap.ContainsKey(id)) PretendMap.Add(id, new(id));
        if (!VisiblePlayers.Contains(id)) VisiblePlayers.Add(id);
    }
    private void NetworkManager_OnClientDisconnectedCallback(ulong obj)
    {
        // Skip if the host disconnected
        if (obj == 0) return;
        UnregisterPlayer(obj);
    }
    private void UnregisterPlayer(ulong id)
    {
        if (StealthMap.ContainsKey(id)) StealthMap.Remove(id);
        if (!PretendMap.ContainsKey(id)) PretendMap.Remove(id);
        if (VisiblePlayers.Contains(id)) VisiblePlayers.Remove(id);
    }

    private void OnVisiblePlayersChanged(NetworkListEvent<ulong> changeEvent)
    {
        Plugin.Logger.LogInfo($"Visible players: [{VisiblePlayers.AsString()}]");

        // The server will be the one updating these values so it is exempt from any actions
        if (IsHostOrServer()) return;
    }

    [ServerRpc(RequireOwnership = false)]
    public void TogglePlayerHiddenServerRpc(ulong playerId)
    {
        if (VisiblePlayers.Contains(playerId)) VisiblePlayers.Remove(playerId);
        else VisiblePlayers.Add(playerId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerMaskAttachedServerRpc(ulong playerId, bool isAttaching)
    {
        SetPlayerMaskAttachedClientRpc(playerId, isAttaching);
    }

    [ClientRpc]
    public void SetPlayerMaskAttachedClientRpc(ulong playerId, bool isAttaching)
    {
        var player = StartOfRound.Instance.GetPlayer(playerId);
        if (player == null
            || player.currentlyHeldObjectServer == null
            || player.currentlyHeldObjectServer is not HauntedMaskItem mask)
        {
            return;
        }

        mask.SetMaskAttached(isAttaching);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerRaisingArmsServerRpc(ulong playerId, bool isRaising)
    {
        SetPlayerMaskAttachedClientRpc(playerId, isRaising);
    }

    [ClientRpc]
    public void SetPlayerRaisingArmsClientRpc(ulong playerId, bool isRaising)
    {
        var player = StartOfRound.Instance.GetPlayer(playerId);
        if (player == null) return;

        player.SetArmsRaised(isRaising);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerMaskEyesServerRpc(ulong playerId, bool isActivating)
    {
        SetPlayerMaskEyesClientRpc(playerId, isActivating);
    }

    [ClientRpc]
    public void SetPlayerMaskEyesClientRpc(ulong playerId, bool isActivating)
    {
        var player = StartOfRound.Instance.GetPlayer(playerId);
        if (player == null
            || player.currentlyHeldObjectServer == null
            || player.currentlyHeldObjectServer is not HauntedMaskItem mask)
        {
            return;
        }

        mask.SetMaskEyes(isActivating);
    }
}