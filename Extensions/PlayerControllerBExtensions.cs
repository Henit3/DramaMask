using GameNetcodeStuff;

namespace DramaMask.Extensions;

public static class PlayerControllerBExtensions
{
    public static ulong GetId(this PlayerControllerB player) => player.actualClientId;
    public static bool IsLocal(this PlayerControllerB player) => player.GetId() == player.NetworkManager.LocalClientId;
}
