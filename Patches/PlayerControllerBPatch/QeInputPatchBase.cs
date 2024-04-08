using DramaMask.Config;
using GameNetcodeStuff;

namespace DramaMask.Patches.PlayerControllerBPatch;

public class QeInputPatchBase
{
    public static bool ShouldProcessInput(PlayerControllerB __instance, string caller,
        ref bool isCustomInput)
    {
        if (!InputUtilsCompat.Enabled) return true;

        // __instance is usually invalid since it doesn't hold currentlyHeldObjectServer until the last invocation
        // We can check against the true value using StartOfRound.Instance.localPlayerController
        var isMaskTarget = __instance != null && __instance.currentlyHeldObjectServer is HauntedMaskItem;
        var player = StartOfRound.Instance.localPlayerController;
        var trueIsMaskTarget = player != null && player.currentlyHeldObjectServer is HauntedMaskItem;

        // Only use the first valid call for custom (usually the last one)
        if (isMaskTarget == trueIsMaskTarget && isCustomInput)
        {
            // Set to false as soon as possible to avoid race condition?
            isCustomInput = false;

            // If on custom keybind, disallow for normal invocation, allow for mask events
            return isMaskTarget;
        }
        // Invalid calls can go through to the usual invocation strategy for compatibility

        // If not on custom keybind, allow for normal invocation, disallow for mask events
        return !isMaskTarget;
    }
}
