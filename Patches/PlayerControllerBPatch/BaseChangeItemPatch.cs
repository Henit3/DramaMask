using DramaMask.Extensions;
using DramaMask.Network;
using GameNetcodeStuff;

namespace DramaMask.Patches.PlayerControllerBPatch;

public abstract class BaseChangeItemPatch
{
    protected static bool ShouldInvoke(PlayerControllerB __instance)
    {
        if (!__instance.isPlayerControlled) return true;

        if (!__instance.IsLocal()) return true;

        if (!NetworkHandler.Instance.MyPretend.IsMaskAttached) return true;

        // Cancel event if all of the above conditions are passed
        return false;
    }
}
