using DramaMask.Extensions;
using DramaMask.Network;
using DramaMask.UI;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace DramaMask.Patches.PlayerControllerBPatch;

[HarmonyPatch(typeof(PlayerControllerB))]
public class KillPlayerPatch
{
    [HarmonyPatch("KillPlayerClientRpc")]
    [HarmonyPrefix]
    public static void Prefix(int playerId, // (playerIndex)
      bool spawnBody, Vector3 bodyVelocity, int causeOfDeath, int deathAnimation)
    {
        PlayerControllerB player = StartOfRound.Instance.allPlayerObjects[playerId]
            .GetComponent<PlayerControllerB>();

        player.isPlayerDead = true;
    }

    [HarmonyPatch(nameof(PlayerControllerB.KillPlayer))]
    [HarmonyPostfix]
    public static void Postfix(PlayerControllerB __instance,
        Vector3 bodyVelocity, bool spawnBody, CauseOfDeath causeOfDeath, int deathAnimation)
    {
        if (!Plugin.Config.UseStealthMeter.Value) return;

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
