using DramaMask.Extensions;
using GameNetcodeStuff;
using HarmonyLib;
using System;

namespace DramaMask.Network;

[HarmonyPatch]
public class NetworkTester
{
    [HarmonyPostfix, HarmonyPatch(typeof(PlayerControllerB), "PlayerJump")]
    static void JumpPatch(PlayerControllerB __instance)
    {
        switch (__instance.currentItemSlot)
        {
            case 0: break;
            case 1: break;
            case 2: break;
            case 3: break;
            default: break;
        }
    }

    [HarmonyPostfix, HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.Crouch))]
    static void CrouchPatch(PlayerControllerB __instance, bool crouch)
    {
        Plugin.Logger.LogDebug($"Data: [{NetworkHandler.Instance.MyStealth}]");
        Plugin.Logger.LogDebug($"Visible: [{NetworkHandler.Instance.VisiblePlayers.AsString()}]");
    }
}