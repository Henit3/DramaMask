using DramaMask.Patches.Base;
using HarmonyLib;

namespace DramaMask.Patches.SandSpiderAIPatch;

[HarmonyPatch(typeof(SandSpiderAI), nameof(SandSpiderAI.DoAIInterval))]
public class DoAIIntervalPatch : ModifyPlayerArrayPatch
{
    [HarmonyPrefix]
    public static void Prefix(SandSpiderAI __instance)
    {
        SaveAndModifyPlayerArray(__instance);
    }

    // TODO: Transpile OOB

    [HarmonyPostfix]
    public static void Postfix(EnemyAI __instance)
    {
        LoadOriginalPlayerArray(__instance);
    }
}