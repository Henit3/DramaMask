using DramaMask.Extensions;
using DramaMask.Network;
using HarmonyLib;

namespace DramaMask.Patches.LethalHandsPatch;

[HarmonyPatch(typeof(LethalHands.LethalHands))]
public class LethalHandsPatch
{
    [HarmonyPatch(nameof(LethalHands.LethalHands.SquareUp))]
    [HarmonyPostfix]
    public static void Postfix(LethalHands.LethalHands __instance)
    {
        var player = GameNetworkManager.Instance.localPlayerController;

        if (!player.IsLocal()) return;
        
        // Only handle the case where the mask is equipped
        if (!NetworkHandler.Instance.MyPretend.IsMaskAttached) return;

        // Detach the mask
        NetworkHandler.Instance.MyPretend.IsMaskAttached = false;

        // Drop it if the config would drop it
        if (LethalHands.NetworkConfig.Instance.itemDropMode is not LethalHands.ItemMode.All) return;

        player.DiscardHeldObject();
    }
}
