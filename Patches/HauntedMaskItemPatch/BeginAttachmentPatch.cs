using DramaMask.Network;
using HarmonyLib;

namespace DramaMask.Patches.HauntedMaskItemPatch;

[HarmonyPatch(typeof(HauntedMaskItem), nameof(HauntedMaskItem.BeginAttachment))]
public class BeginAttachmentPatch
{
    [HarmonyPrefix]
    public static bool Prefix(HauntedMaskItem __instance)
    {
        return !NetworkHandler.Instance.MyPretend.IsMaskAttached
            || Plugin.Config.AttachedCanPossess.Value;
    }
}
