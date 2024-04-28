using DramaMask.Network;
using GameNetcodeStuff;
namespace DramaMask.Extensions;

public static class PlayerControllerBExtensions
{
    public static ulong GetId(this PlayerControllerB player) => player?.playerClientId ?? 0;
    public static bool IsLocal(this PlayerControllerB player) => player.GetId() == HUDManager.Instance.localPlayer.playerClientId;

    // We assume that the HoldMask animation has been overriden by the attach mask handler
    public static void SetArmsRaised(this PlayerControllerB player, bool isRaising) {} //player.SafeSetAnimation("HoldMask", isRaising);
    public static bool SafeSetAnimation(this PlayerControllerB player, string animation, bool value)
    {
        if (player == null)
        {
            Plugin.Logger.LogWarning($"No player found to set animation {animation} on.");
            return false;
        }
        player.playerBodyAnimator.SetBool(animation, value: value);
        return true;
    }

    public static bool IsHidden(this PlayerControllerB player)
    {
        return NetworkHandler.Instance.VisiblePlayers != null
            && !NetworkHandler.Instance.VisiblePlayers.Contains(player.GetId());
    }
}
