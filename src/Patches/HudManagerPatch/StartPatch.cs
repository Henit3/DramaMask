using DramaMask.Constants;
using DramaMask.UI;
using DunGen;
using HarmonyLib;
using System.Collections;
using UnityEngine;

namespace DramaMask.Patches.HudManagerPatch;

[HarmonyPatch(typeof(HUDManager), "Start")]
public class StartPatch
{
    [HarmonyPostfix]
    public static void Postfix(HUDManager __instance)
    {
        CoroutineHelper.Start(CreateStealthMeter());
    }

    private static IEnumerator CreateStealthMeter()
    {
        yield return new WaitUntil(() => GameNetworkManager.Instance.localPlayerController?.sprintMeterUI != null);

        if (!Plugin.Config.UseStealthMeter.Value
            || Plugin.Config.StealthMeterVisibility.LocalValue is MeterVisibility.Never) yield break;

        StealthMeter.Init();

        StealthMeter.Instance.Visible = Plugin.Config.StealthMeterVisibility.LocalValue is MeterVisibility.Always;
    }
}