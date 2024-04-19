using HarmonyLib;
using UnityEngine;

namespace DramaMask.Patches.PlayerControllerBPatch;

[HarmonyPatch(typeof(GameNetworkManager), "SaveItemsInShip")]
public class SaveItemsInShipPatch
{
    [HarmonyPrefix]
    public static void Prefix(GameNetworkManager __instance)
    {
        var masks = Object.FindObjectsByType<HauntedMaskItem>(FindObjectsSortMode.None);
        foreach (var mask in masks)
        {
            // Stop headmask persisting in save file (host disconnection) after player is invalid
            if (mask == null) continue;
            if (mask.currentHeadMask == null) continue;

            var headMask = mask.currentHeadMask.gameObject.GetComponent<HauntedMaskItem>();
            if (headMask == null) continue;

            // Set to not save & get cleaned up the host exiting the game
            // Note: saves will be incorrect until host disconnects; i.e. ship autosave
            if (!__instance.isDisconnecting) return;

            headMask.deactivated = true;
            Object.Destroy(mask.currentHeadMask.gameObject);
            mask.currentHeadMask = null;
        }
    }
}
