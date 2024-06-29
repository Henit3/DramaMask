using DramaMask.Extensions;
using DramaMask.Models.Network;
using GameNetcodeStuff;
using System.Collections.Generic;
using Unity.Netcode;

namespace DramaMask.Network;

public class NetworkHandler : NetworkBehaviour
{
    public static NetworkHandler Instance { get; private set; }

    public NetworkList<ulong> VisiblePlayers;

    public StealthData GetStealth(bool isLocal, ulong id) => StealthMap.GetNetworkData(MyStealth, isLocal, id);
    public Dictionary<ulong, StealthData> StealthMap { get; private set; }
    public StealthData MyStealth { get; private set; }

    public PretendData GetPretend(bool isLocal, ulong id) => PretendMap.GetNetworkData(MyPretend, isLocal, id);
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

        if (VisiblePlayers == null)
        {
            Plugin.Logger.LogWarning("Visible player list could not unregister player: This is fine on host disconnection");
            return;
        }

        if (VisiblePlayers.Contains(id)) VisiblePlayers.Remove(id);
    }

    private void OnVisiblePlayersChanged(NetworkListEvent<ulong> changeEvent)
    {
        Plugin.Logger.LogInfo($"Visible players: [{VisiblePlayers.AsString()}]");

        // The server will be the one updating these values so it is exempt from any actions
        if (IsHostOrServer()) return;
    }

    public void SetPlayerHiddenServer(ulong playerId, bool isHidden)
    {
        if (isHidden && VisiblePlayers.Contains(playerId)) VisiblePlayers.Remove(playerId);
        else if (!isHidden && !VisiblePlayers.Contains(playerId)) VisiblePlayers.Add(playerId);
    }

    public void SetPlayerMaskAttachedServer(ulong playerId, bool isAttaching)
    {
        SetPlayerMaskAttachedClientRpc(playerId, isAttaching);
    }
    [ClientRpc]
    public void SetPlayerMaskAttachedClientRpc(ulong playerId, bool isAttaching)
    {
        var player = StartOfRound.Instance.GetPlayer(playerId);
        Plugin.Logger.LogDebug($"AttachMaskClientRPC {playerId} -> {player}: {isAttaching}");
        if (!IsMaskActionValid(player, out var mask, out var reasonInvalid))
        {
            Plugin.Logger.LogDebug($"AttachMaskClientRPC invalid call: {reasonInvalid}");
            return;
        }

        mask.SetMaskAttached(isAttaching);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerMaskEyesValueServerRpc(ulong playerId, bool isActivating)
    {
        PretendMap[playerId].IsMaskEyesOn = isActivating;
    }
    public void SetPlayerMaskEyesServer(ulong playerId, bool isActivating)
    {
        SetPlayerMaskEyesClientRpc(playerId, isActivating);
    }
    [ClientRpc]
    public void SetPlayerMaskEyesClientRpc(ulong playerId, bool isActivating)
    {
        var player = StartOfRound.Instance.GetPlayer(playerId);
        Plugin.Logger.LogDebug($"MaskEyesClientRPC {playerId} -> {player}: {isActivating}");
        if (!IsMaskActionValid(player, out var mask, out var reasonInvalid))
        {
            Plugin.Logger.LogDebug($"MaskEyesClientRPC invalid call: {reasonInvalid}");
            return;
        }

        mask.SetMaskEyes(isActivating);
    }

    private bool IsMaskActionValid(PlayerControllerB player, out HauntedMaskItem mask, out string reasonInvalid)
    {
        reasonInvalid = null;
        mask = null;

        if (player == null)
        {
            reasonInvalid = "Player is null";
            return false;
        }

        var currentlyHeldObjectServer = player.currentlyHeldObjectServer;
        if (currentlyHeldObjectServer == null)
        {
            reasonInvalid = "CurrentlyHeldObjectServer is null";
            return false;
        }

        if (currentlyHeldObjectServer is not HauntedMaskItem maskItem)
        {
            reasonInvalid = "CurrentlyHeldObjectServer is not a HauntedMaskItem";
            return false;
        }

        mask = maskItem;
        return true;
    }
}