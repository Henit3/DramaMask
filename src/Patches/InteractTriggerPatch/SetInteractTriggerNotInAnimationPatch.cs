using HarmonyLib;

namespace DramaMask.Patches.PlayerControllerBPatch;

[HarmonyPatch(typeof(InteractTrigger), nameof(InteractTrigger.SetInteractTriggerNotInAnimation))]
public class SetInteractTriggerNotInAnimationPatch : BaseEndSpecialAnimationPatch
{
    [HarmonyPostfix]
    public static void Postfix(InteractTrigger __instance, int playerUsing)
    {
        if (StartOfRound.Instance.allPlayerScripts.Length <= playerUsing) return;

        HideMaskVisibilityOnAnimationEnd(StartOfRound.Instance.allPlayerScripts[playerUsing]);
    }
}
