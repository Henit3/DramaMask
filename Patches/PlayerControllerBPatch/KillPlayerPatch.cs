using DramaMask.Extensions;
using DramaMask.Network;
using DramaMask.UI;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace DramaMask.Patches.PlayerControllerBPatch;

[HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.KillPlayer))]
public class KillPlayerPatch
{
    [HarmonyPostfix]
    public static void Postfix(PlayerControllerB __instance,
        Vector3 bodyVelocity, bool spawnBody, CauseOfDeath causeOfDeath, int deathAnimation)
    {
        if (!ConfigValues.UseStealthMeter) return;

        var id = __instance.GetId();

        if (__instance.IsLocal())
        {
            NetworkHandler.Instance.MyStealth.Reset();
            NetworkHandler.Instance.MyPretend.Reset();
            StealthMeterUI.Instance.Visible = false;
        }
        if (NetworkHandler.IsHostOrServer())
        {
            NetworkHandler.Instance.StealthMap[id].Reset();
            NetworkHandler.Instance.PretendMap[id].Reset();
        }
    }
}
