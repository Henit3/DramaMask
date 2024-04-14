using DramaMask.Config;
using GameNetcodeStuff;

namespace DramaMask.Patches.PlayerControllerBPatch;

public class QeInputPatchBase
{
    public static bool ShouldProcessInput(PlayerControllerB __instance,
        ref bool isCustomInput,
        bool right)
    {
        if (!InputUtilsCompat.Enabled) return true;

        // Return true if custom clashes with default
        if ((right && InputUtilsCompat.IsMaskAttachDefaultClash())
            || (!right && InputUtilsCompat.IsMaskEyeDefaultClash())) return true;

        // __instance is usually invalid since it doesn't hold currentlyHeldObjectServer until the last invocation
        // We can check against the true value using StartOfRound.Instance.localPlayerController
        var isMaskTarget = __instance != null && __instance.currentlyHeldObjectServer is HauntedMaskItem;
        var player = StartOfRound.Instance.localPlayerController;
        var trueIsMaskTarget = player != null && player.currentlyHeldObjectServer is HauntedMaskItem;

        // Invalid calls are treated as non custom inputs for compatibility
        var wasCustomInput = false;
        // Only use the first valid call for custom (usually the last one)
        if (isMaskTarget == trueIsMaskTarget)
        {
            // Set to false as soon as possible to avoid race condition?
            wasCustomInput = isCustomInput;
            isCustomInput = false;
        }

        // Allow custom input on masks, and default inputs on non-masks
        return wasCustomInput ^ !isMaskTarget;
    }
}
