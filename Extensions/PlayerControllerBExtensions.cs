using GameNetcodeStuff;
namespace DramaMask.Extensions;

public static class PlayerControllerBExtensions
{
    public static ulong GetId(this PlayerControllerB player) => player?.actualClientId ?? 0;
    public static bool IsLocal(this PlayerControllerB player) => player.GetId() == (player?.NetworkManager?.LocalClientId ?? 0);

    public static void SetArmsRaised(this PlayerControllerB player, bool isRaising) => player.SafeSetAnimation("HandsOut", isRaising);
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
}
