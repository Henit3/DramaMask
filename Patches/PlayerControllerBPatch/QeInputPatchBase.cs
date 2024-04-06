using DramaMask.Config;
using GameNetcodeStuff;

namespace DramaMask.Patches.PlayerControllerBPatch;

public class QeInputPatchBase
{
    public static bool ShouldProcessInput(PlayerControllerB __instance, ref bool isCustomInput)
    {
        if (!InputUtilsCompat.Enabled) return true;

        var isMaskTarget = __instance != null
                && __instance.currentlyHeldObjectServer is HauntedMaskItem;

        // If not on custom keybind, allow for normal invocation, disallow for mask events
        if (!isCustomInput) return !isMaskTarget;

        // If on custom keybind, disallow for normal invocation, allow for mask events
        if (!isMaskTarget) return false;

        isCustomInput = false;
        return true;
    }
}
