using DramaMask.Config;
using DramaMask.Extensions;
using GameNetcodeStuff;
using HarmonyLib;

namespace DramaMask.Patches.Enemies.SandSpiderAIPatch;

[HarmonyPatch(typeof(SandSpiderAI), nameof(SandSpiderAI.TriggerChaseWithPlayer))]
public class TriggerChaseWithPlayerPatch
{
    [HarmonyPrefix]
    public static bool Prefix(SandSpiderAI __instance,
        PlayerControllerB playerScript)
    {
        if (!EnemyTargetHandler.ShouldHideFromEnemy(nameof(SandSpiderAI))) return true;

        return !playerScript.IsHidden();
    }
}