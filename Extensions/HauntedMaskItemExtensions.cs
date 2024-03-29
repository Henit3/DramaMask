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

    private static bool _headMaskTransformsLoaded = false;
    private static Vector3 _originalHeadMaskPos;
    private static readonly Vector3 _localHeadMaskPosAdjustment = new(-0.026f, -0.02f, -0.03f);
    private static readonly Vector3 _publicHeadMaskPosAdjustment = new(-0.03f, -0.02f, -0.03f);
    private static Vector3 _originalHeadMaskRot;
    private static readonly Vector3 _localHeadMaskRotAdjustment = new(-5f, 0f, -2f);
    private static readonly Vector3 _publicHeadMaskRotAdjustment = new(-5f, 0f, 0f);
    private static Vector3 _originalHeadMaskScale;

    /*public static AnimationClip HoldingMaskAnimation;
    public static AnimationClip ArmsOutAnimation;*/

    public static void SetVisibility(this HauntedMaskItem mask, bool isVisible, bool toOutline = true)
    {
        // Register original mesh for each mash type to reset with later
        if (!_originalMeshMap.ContainsKey(mask.maskTypeId))
        {
            _originalMeshMap.Add(mask.maskTypeId,
                mask.gameObject.GetComponentsInChildren<MeshFilter>().First(IsMainMesh).mesh);
        }

        // Set visibility for all main renderers
        var renderers = mask.gameObject.GetComponentsInChildren<MeshRenderer>()
            .Where(IsMainMesh).ToList();
        foreach (var renderer in renderers.Skip(1)) renderer.enabled = isVisible;

        // Set completely invisible if not outlining
        renderers.First().enabled = !(!toOutline && !isVisible);

        if (toOutline)
        {
            // Set main mesh filter's mesh
            var maskMeshFilter = mask.gameObject.GetComponentsInChildren<MeshFilter>().First(IsMainMesh);
            maskMeshFilter.mesh = ConfigValues.SeeWornMaskOutline && !isVisible
                ? OutlineMesh
                : _originalMeshMap[mask.maskTypeId];
        }
    }

    private static bool IsMainMesh(Component component) => !(component.name is "EyesFilled" || component.name.StartsWith("ScanNode"));

    public static void SetMaskAttached(this HauntedMaskItem mask, bool isAttaching)
    {
        if (!isAttaching) NetworkHandler.Instance.MyPretend.IsMaskEyesOn = false;

        //Based on HauntedMaskItem.MaskClampToHeadAnimationEvent
        var player = AccessTools.Field(typeof(HauntedMaskItem), "previousPlayerHeldBy").GetValue(mask) as PlayerControllerB;

        // Set held mask visibility based on attach status
        mask.enabled = !isAttaching;
        mask.SetVisibility(!isAttaching, toOutline: false);

        /*if (player != null)
        {
            var overrideController = player.playerBodyAnimator.runtimeAnimatorController as AnimatorOverrideController;
            if (overrideController == null)
            {
                Plugin.Logger.LogWarning("Player animator override controller not accessible");
            }
            else
            {
                overrideController["HoldMaskToFace"] = isAttaching ? ArmsOutAnimation : HoldingMaskAnimation;
                Plugin.Logger.LogDebug($"Overwrote HoldMask to {overrideController["HoldMaskToFace"].name}");
            }
        }*/

        if (isAttaching)
        {
            if (player != null)
            {
                mask.currentHeadMask = Object.Instantiate(mask.gameObject, null).transform;

                AdjustMaskOffsets(mask, player.IsLocal());

                AccessTools.Method(typeof(HauntedMaskItem), "PositionHeadMaskWithOffset").Invoke(mask, null);

                AccessTools.Field(typeof(HauntedMaskItem), "clampedToHead").SetValue(mask, true);

                // Set head mask invisibile only for the local player (if they have the config)
                mask.currentHeadMask.gameObject.GetComponent<HauntedMaskItem>()
                    .SetVisibility(!player.IsLocal());
            }

            player.SafeSetAnimation("Grab", false);
            player.SafeSetAnimation("cancelHolding", true);
        }
        else
        {
            if (mask.currentHeadMask != null)
            {
                Object.Destroy(mask.currentHeadMask.gameObject);
                mask.currentHeadMask = null;
            }

            AccessTools.Field(typeof(HauntedMaskItem), "clampedToHead").SetValue(mask, false);

            player.SafeSetAnimation("cancelHolding", false);
            player.SafeSetAnimation("Grab", true);
        }

        if (player.IsLocal()) mask.SetControlTipsForItem();
    }

    private static void AdjustMaskOffsets(this HauntedMaskItem mask, bool isLocal)
    {
        var headMask = mask.currentHeadMask.gameObject.GetComponent<HauntedMaskItem>();

        if (!_headMaskTransformsLoaded)
        {
            // Assumes all mask types have the same offset
            _headMaskTransformsLoaded = true;
            _originalHeadMaskPos = mask.headPositionOffset;
            _originalHeadMaskRot = mask.headRotationOffset;
            _originalHeadMaskScale = headMask.transform.localScale;
        }

        mask.headPositionOffset = _originalHeadMaskPos + (isLocal
            ? _localHeadMaskPosAdjustment
            : _publicHeadMaskPosAdjustment);
        mask.headRotationOffset += _originalHeadMaskRot + (isLocal
            ? _localHeadMaskRotAdjustment
            : _publicHeadMaskRotAdjustment);
        headMask.transform.localScale = _originalHeadMaskScale * 0.83f;
    }

    public static void SetMaskEyes(this HauntedMaskItem mask, bool toActivate)
    {
        var eyesFilled = mask.currentHeadMask != null
            ? mask.currentHeadMask.gameObject.GetComponent<HauntedMaskItem>().maskEyesFilled
            : mask.maskEyesFilled;
        eyesFilled.enabled = toActivate;

        if (mask.playerHeldBy.IsLocal()) mask.SetControlTipsForItem();
    }
}
