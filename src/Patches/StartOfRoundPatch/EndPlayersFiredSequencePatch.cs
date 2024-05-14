using DramaMask.Network;
using HarmonyLib;

namespace DramaMask.Patches.StartOfRoundPatch;

[HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.EndPlayersFiredSequenceClientRpc))]
public class EndPlayersFiredSequencePatch
{
    [HarmonyPostfix]
    public static void Postfix(StartOfRound __instance)
    {
        var wasMaskAttached = NetworkHandler.Instance.MyPretend.IsMaskAttached;
        NetworkHandler.Instance.MyPretend.IsMaskAttached = false;

        if (wasMaskAttached)
        {
            var curItemSlot = __instance.localPlayerController.currentItemSlot;
            HUDManager.Instance.itemSlotIcons[curItemSlot].enabled = false;
        }

        NetworkHandler.Instance.MyPretend.Reset();
        NetworkHandler.Instance.MyStealth.Reset();

        if (!NetworkHandler.IsHostOrServer()) return;

        foreach (var stealthData in NetworkHandler.Instance.StealthMap) stealthData.Value.Reset();
        foreach (var pretendData in NetworkHandler.Instance.PretendMap) pretendData.Value.Reset();
    }
}
