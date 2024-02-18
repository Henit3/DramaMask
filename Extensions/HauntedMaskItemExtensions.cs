using DramaMask.Network;
using GameNetcodeStuff;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DramaMask.Extensions;

public static class HauntedMaskItemExtensions
{
    public static bool CanHide(this HauntedMaskItem mask) => mask != null && (ConfigValues.AllMasksHide || mask.maskTypeId == 6);

    public static Mesh OutlineMesh { get; set; } = null;
    private static readonly Dictionary<int, Mesh> _originalMeshMap = new();
    public static void SetOutlineView(this HauntedMaskItem mask, bool toOutline)
    {
        if (!ConfigValues.SeeWornMaskOutline) return;

        // Register original mesh for each mash type to reset with later
        if (!_originalMeshMap.ContainsKey(mask.maskTypeId))
        {
            _originalMeshMap.Add(mask.maskTypeId,
                mask.gameObject.GetComponentsInChildren<MeshFilter>().First(IsMainMesh).mesh);
        }

        // Set visibility for all main renderers
        var renderers = mask.gameObject.GetComponentsInChildren<MeshRenderer>()
            .Where(IsMainMesh).ToList();
        foreach (var renderer in renderers.Skip(1)) renderer.enabled = !toOutline;

        // Set main mesh filter's mesh
        var maskMeshFilter = mask.gameObject.GetComponentsInChildren<MeshFilter>().First(IsMainMesh);
        maskMeshFilter.mesh = toOutline
            ? OutlineMesh
            : _originalMeshMap[mask.maskTypeId];
    }

    private static bool IsMainMesh(Component component) => !(component.name is "EyesFilled" || component.name.StartsWith("ScanNode"));

    public static void SetMaskAttached(this HauntedMaskItem mask, bool isAttaching)
    {
        if (!isAttaching) NetworkHandler.Instance.MyPretend.IsMaskEyesOn = false;

        //Based on HauntedMaskItem.MaskClampToHeadAnimationEvent
        if (!ResetHoldingAnimation(mask)) return;

        // Set held mask visibility based on attach status
        mask.enabled = !isAttaching;

        if (isAttaching)
        {
            mask.currentHeadMask = Object.Instantiate(mask.gameObject, null).transform;
            AccessTools.Method(typeof(HauntedMaskItem), "PositionHeadMaskWithOffset").Invoke(mask, null);

            AccessTools.Field(typeof(HauntedMaskItem), "clampedToHead").SetValue(mask, true);

            mask.currentHeadMask.gameObject.GetComponent<HauntedMaskItem>().SetOutlineView(true);
        }
        else
        {
            Object.Destroy(mask.currentHeadMask.gameObject);
            mask.currentHeadMask = null;

            AccessTools.Field(typeof(HauntedMaskItem), "clampedToHead").SetValue(mask, false);
        }
    }

    private static bool ResetHoldingAnimation(HauntedMaskItem mask)
    {
        var player = AccessTools.Field(typeof(HauntedMaskItem), "previousPlayerHeldBy").GetValue(mask) as PlayerControllerB;
        if (player == null)
        {
            Plugin.Logger.LogError("No player found to attach mask to.");
            return false;
        }
        player.playerBodyAnimator.SetBool("HoldMask", value: false);
        return true;
    }

    public static void SetMaskEyes(this HauntedMaskItem mask, bool toActivate)
    {
        if (mask.currentHeadMask != null)
        {
            mask.currentHeadMask.gameObject.GetComponent<HauntedMaskItem>()
                .maskEyesFilled.enabled = toActivate;
        }
        mask.maskEyesFilled.enabled = toActivate;
    }
}
