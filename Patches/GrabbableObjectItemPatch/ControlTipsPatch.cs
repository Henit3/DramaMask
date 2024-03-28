using DramaMask.Extensions;
using DramaMask.Network;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;

namespace DramaMask.Patches.GrabbableObjectItemPatch;

[HarmonyPatch(typeof(GrabbableObject), nameof(GrabbableObject.SetControlTipsForItem))]
public class ControlTipsPatch
{
    private static List<string> _defaultTooltips;

    [HarmonyPostfix]
    public static void Postfix(GrabbableObject __instance)
    {
        if (__instance is not HauntedMaskItem mask
            || mask.playerHeldBy == null
            || !mask.IsOwner
            || mask.itemProperties == null
            || mask.itemProperties.toolTips == null
            || !mask.CanHide())
        {
            return;
        }

        _defaultTooltips ??= mask.itemProperties.toolTips.ToList();
        var newTooltips = _defaultTooltips.ToList();
        
        if (NetworkHandler.Instance.MyPretend.IsMaskAttached)
        {
            newTooltips.AddRange([
                "Detach mask: [Q]",
                !NetworkHandler.Instance.MyPretend.IsMaskEyesOn
                    ? "Activate eyes: [E]"
                    : "Deactivate eyes: [E]"
            ]);
        }
        else
        {
            newTooltips.AddRange([
                "Attach mask: [Q]",
                "" // Hack to stop showing eye controls on detaching
            ]);
        }

        mask.itemProperties.toolTips = newTooltips.ToArray();
        HUDManager.Instance.ChangeControlTipMultiple(mask.itemProperties.toolTips, true, mask.itemProperties);
    }
}
