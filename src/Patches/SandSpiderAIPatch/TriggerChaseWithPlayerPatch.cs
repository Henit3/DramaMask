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
        // TODO: Optional behaviour to ignore this

        return !playerScript.IsHidden();
    }
}