namespace DramaMask.Network.Models;

public class NetworkData
{
    protected readonly bool IsClientCopy;
    protected readonly ulong PlayerId;
    public NetworkData()
    {
        IsClientCopy = true;
    }
    public NetworkData(ulong playerId)
    {
        PlayerId = playerId;
    }

    protected bool ShouldCopyToMap() => IsClientCopy && NetworkHandler.IsHostOrServer();
    public override string ToString() => $"{(IsClientCopy ? "Self" : PlayerId)}";
}
