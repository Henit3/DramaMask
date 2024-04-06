using DramaMask.Config;
using GameNetcodeStuff;
using HarmonyLib;

namespace DramaMask.Patches.PlayerControllerBPatch;

[HarmonyPatch(typeof(PlayerControllerB), "ItemTertiaryUse_performed")]
public class TertiaryUsePatch : QeInputPatchBase
{
    [HarmonyPrefix]
    public static bool Prefix(PlayerControllerB __instance)
        => ShouldProcessInput(__instance, ref InputUtilsCompat.HandleMaskEyes);

    [HarmonyPostfix]
    public static void Postfix(PlayerControllerB __instance)
        => InputUtilsCompat.HandleMaskEyes = false;
}
