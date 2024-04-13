using DramaMask.Extensions;
using GameNetcodeStuff;

namespace DramaMask.Patches.PlayerControllerBPatch;

public class BaseEndSpecialAnimationPatch
{
    public static void HideMaskVisibilityOnAnimationEnd(PlayerControllerB player)
    {
        if (player == null
            || player.currentlyHeldObjectServer == null
            || player.currentlyHeldObjectServer is not HauntedMaskItem mask
            || mask.currentHeadMask == null)
        {
            return;
        }

        mask.SetVisibility(false);
    }
}
