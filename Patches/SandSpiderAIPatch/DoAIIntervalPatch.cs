using DramaMask.Patches.EnemyAIPatch;
using HarmonyLib;

namespace DramaMask.Patches.SandSpiderAIPatch;

[HarmonyPatch(typeof(SandSpiderAI), nameof(SandSpiderAI.DoAIInterval))]
public class DoAIIntervalPatch : BaseModifyPlayerArrayPatch
{
    [HarmonyPrefix]
    public static void Prefix(SandSpiderAI __instance)
    {
        SaveAndModifyPlayerArray(__instance);
    }

    [HarmonyPostfix]
    public static void Postfix(EnemyAI __instance)
    {
        LoadOriginalPlayerArray(__instance);
    }
}