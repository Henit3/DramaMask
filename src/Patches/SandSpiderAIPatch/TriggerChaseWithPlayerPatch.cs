using DramaMask.Constants;
using DramaMask.Extensions;
using GameNetcodeStuff;
using HarmonyLib;

namespace DramaMask.Patches.SandSpiderAIPatch;

[HarmonyPatch(typeof(SandSpiderAI), nameof(SandSpiderAI.TriggerChaseWithPlayer))]
public class TriggerChaseWithPlayerPatch
{
    [HarmonyPrefix]
    public static bool Prefix(SandSpiderAI __instance,
        PlayerControllerB playerScript)
    {
        if (!EnemyTargets.HidesFromEnemy(nameof(SandSpiderAI))) return true;

        return !playerScript.IsHidden();
    }
}