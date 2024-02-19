using DramaMask.Extensions;
using DramaMask.Network;
using GameNetcodeStuff;
using HarmonyLib;

namespace DramaMask.Patches.PlayerControllerBPatch;

[HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.DropAllHeldItems))]
public class DropAllHeldItemsPatch : BaseChangeItemPatch
{
    [HarmonyPrefix]
    public static void Prefix(PlayerControllerB __instance, bool itemsFall = true, bool disconnecting = false)
    {
        if (!disconnecting) return;

        // Do this on the server since the player who's doing this will disconnect anyway
        if (!NetworkHandler.IsHostOrServer()) return;

        // If player had mask attached, detach the mask before it is dropped
        var id = __instance.GetId();
        var targetData = NetworkHandler.Instance.PretendMap[id];
        if (targetData.IsMaskAttached) targetData.IsMaskAttached = false;
    }
}
