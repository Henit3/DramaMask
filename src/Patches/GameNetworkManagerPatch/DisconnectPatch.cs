using DramaMask.UI;
using HarmonyLib;

namespace DramaMask.Patches.PlayerControllerBPatch;

[HarmonyPatch(typeof(GameNetworkManager), nameof(GameNetworkManager.Disconnect))]
public class DisconnectPatch
{
    [HarmonyPostfix]
    public static void Postfix(GameNetworkManager __instance)
    {
        if (StealthMeter.Initialised) StealthMeter.Initialised = false;
    }
}
