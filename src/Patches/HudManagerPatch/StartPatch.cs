using DramaMask.Constants;
using DramaMask.UI;
using HarmonyLib;

namespace DramaMask.Patches.HudManagerPatch;

[HarmonyPatch(typeof(HUDManager), "Start")]
public class StartPatch
{
    [HarmonyPostfix]
    public static void Postfix(HUDManager __instance)
    {
        if (!Plugin.Config.UseStealthMeter.Value
            || Plugin.Config.StealthMeterVisibility.LocalValue is MeterVisibility.Never) return;

        StealthMeter.Init();

        StealthMeter.Instance.Visible = Plugin.Config.StealthMeterVisibility.LocalValue is MeterVisibility.Always;
    }
}