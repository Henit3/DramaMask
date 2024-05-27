using DramaMask.Constants;
using DramaMask.UI;
using HarmonyLib;

namespace DramaMask.Patches.HudManagerPatch;

[HarmonyPatch(typeof(HUDManager), "Awake")]
public class AwakePatch
{
    [HarmonyPostfix]
    public static void Postfix(HUDManager __instance, ref HUDElement[] ___HUDElements)
    {
        if (!Plugin.Config.UseStealthMeter.Value
            || Plugin.Config.StealthMeterVisibility.LocalValue is MeterVisibility.Never) return;

        StealthMeter.Init();

        StealthMeter.Instance.Visible = Plugin.Config.StealthMeterVisibility.LocalValue is MeterVisibility.Always;
    }
}