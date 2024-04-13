using DramaMask.Extensions;
using GameNetcodeStuff;
using HarmonyLib;

namespace DramaMask.Patches.SandSpiderAIPatch;

[HarmonyPatch(typeof(SandSpiderAI), nameof(SandSpiderAI.TriggerChaseWithPlayer))]
public class TriggerChaseWithPlayerPatch
{
    [HarmonyPrefix]
    public static bool Prefix(SandSpiderAI __instance, ref bool __result,
        PlayerControllerB playerScript)
    {
        // TODO: Optional behaviour to ignore this

        if (!playerScript.IsHidden()) return true;

        // Override with can't find the player if the player is hiding
        __result = false;
        return false;
    }
}