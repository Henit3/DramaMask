﻿using DramaMask.Network;

namespace DramaMask.Models.Network;

/**
 * Core part of the networking architecture:
 * Each client will have its own instance but the server will store moderated copies of these.
 * All changes to other clients' data will need to go through the server host before being sent back,
 *  either through Network Variables or RPCs, to allow extensible moderation of rogue requests.
 */
public abstract class NetworkData
{
    protected readonly bool IsLocalCopy;
    protected readonly ulong PlayerId;
    public NetworkData()
    {
        IsLocalCopy = true;
    }
    public NetworkData(ulong playerId)
    {
        PlayerId = playerId;
    }

    public abstract void Reset();

    protected bool ShouldCopyToMap() => IsLocalCopy && NetworkHandler.IsHostOrServer();
    protected bool ShouldServerProcess() => !IsLocalCopy && NetworkHandler.IsHostOrServer();
    public override string ToString() => $"{(IsLocalCopy ? "Self" : PlayerId)}";
}
