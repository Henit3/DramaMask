using DramaMask.Extensions;
using DramaMask.Network;
using HarmonyLib;

namespace DramaMask.Patches.GrabbableObjectItemPatch;

[HarmonyPatch(typeof(GrabbableObject), "InteractLeftRightServerRpc")]
public class InteractLeftRightPatch
{
    [HarmonyPostfix]
    public static void Postfix(GrabbableObject __instance, bool right)
    {
        if (__instance is not HauntedMaskItem instance) return;

        if (instance.playerHeldBy == null) return;

        // Continue only if the server (behaviour), or the local player (UI)
        if (!(NetworkHandler.IsHostOrServer() || instance.playerHeldBy.IsLocal())) return;

        var id = instance.playerHeldBy.GetId();

        var targetPretendData = instance.playerHeldBy.IsLocal()
            ? NetworkHandler.Instance.MyPretend
            : NetworkHandler.Instance.PretendMap[id];

        if (targetPretendData.IsMaskAttached)
        {
            if (!right)
            {
                targetPretendData.IsMaskAttached = false;
            }
        }
        else
        {
            if (!right)
            {
                targetPretendData.IsMaskAttached = true;
            }
            else
            {
                targetPretendData.IsMaskEyesOn = !targetPretendData.IsMaskEyesOn;
            }
        }
    }
}
