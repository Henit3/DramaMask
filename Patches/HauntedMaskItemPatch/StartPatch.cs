using HarmonyLib;

namespace DramaMask.Patches.HauntedMaskItemPatch;

[HarmonyPatch(typeof(GrabbableObject), nameof(GrabbableObject.Start))]
public class StartPatch
{
    [HarmonyPostfix]
    public static void Postfix(GrabbableObject __instance)
    {
        if (__instance is not HauntedMaskItem) return;

        // Allow HauntedMask usage to sync across clients so it can be tracked on the server
        __instance.itemProperties.syncUseFunction = true;
    }
}
