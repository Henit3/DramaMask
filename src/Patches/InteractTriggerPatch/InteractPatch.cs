using DramaMask.Extensions;
using DramaMask.Network;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace DramaMask.Patches.PlayerControllerBPatch;

[HarmonyPatch(typeof(InteractTrigger), nameof(InteractTrigger.Interact))]
public class InteractPatch
{
    private const string StorageClosetName = "StorageCloset";
    private const string DepositCounterName = "DepositCounter";

    [HarmonyPrefix]
    public static bool Prefix(InteractTrigger __instance, Transform playerTransform)
    {
        // Stop triggering the storage closet or desposit counter while hidden (stops depositing the mask)
        if (__instance.transform.parent is null
            || __instance.transform.parent.parent is null) return true;

        if (__instance.transform.parent.parent.name != StorageClosetName
            && __instance.transform.parent.parent.name != DepositCounterName) return true;

        var player = playerTransform.GetComponent<PlayerControllerB>();
        var stealthData = NetworkHandler.Instance.GetStealth(player.IsLocal(), player.GetId());
        if (stealthData == null
            || !stealthData.IsAttemptingStealth()) return true;

        HUDManager.Instance.DisplayStatusEffect("Cannot interact with this object while using a mask");
        return false;
    }
}