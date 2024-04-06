using DramaMask.Config;
using GameNetcodeStuff;
using HarmonyLib;

namespace DramaMask.Patches.PlayerControllerBPatch;

[HarmonyPatch(typeof(PlayerControllerB), "ItemSecondaryUse_performed")]
public class SecondaryUsePatch : QeInputPatchBase
{
    [HarmonyPrefix]
    public static bool Prefix(PlayerControllerB __instance)
        => ShouldProcessInput(__instance, ref InputUtilsCompat.HandleAttachMask);

    [HarmonyPostfix]
    public static void Postfix(PlayerControllerB __instance)
        => InputUtilsCompat.HandleAttachMask = false;
}
