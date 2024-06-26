using DramaMask.Network;
using DramaMask.Constants;
using GameNetcodeStuff;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DramaMask.Extensions;

public static class HauntedMaskItemExtensions
{
    public static bool CanHide(this HauntedMaskItem mask) => mask != null && (Plugin.Config.AllMasksHide.Value || mask.maskTypeId == 6);

    public static Mesh OutlineMesh { get; set; } = null;
    private static readonly Dictionary<int, Mesh> _originalMeshMap = new();
    public static Material TransparentMat { get; set; } = null;
    private static readonly Dictionary<int, Material> _originalMatMap = new();

    private static bool _headMaskTransformsLoaded = false;
    private static Vector3 _originalHeadMaskPos;
    private static readonly Vector3 _localHeadMaskPosAdjustment = new(-0.028f, -0.02f, -0.03f);
    private static readonly Vector3 _publicHeadMaskPosAdjustment = new(-0.03f, -0.02f, -0.03f);
    private static Vector3 _originalHeadMaskRot;
    private static readonly Vector3 _localHeadMaskRotAdjustment = new(-5f, 0f, -2f);
    private static readonly Vector3 _publicHeadMaskRotAdjustment = new(-5f, 0f, 0f);
    private static Vector3 _originalHeadMaskScale;

    public static void SetVisibility(this HauntedMaskItem mask,
        bool isVisible, bool? isOthersVisible = null)
    {
        isOthersVisible ??= isVisible;

        // Set visibility for all main mask renderers
        var renderers = mask.gameObject.GetComponentsInChildren<MeshRenderer>()
            .Where(IsMainMaskMesh).ToList();
        foreach (var renderer in renderers.Skip(1)) renderer.enabled = isOthersVisible.Value;

        // Set main renderer visibility
        renderers.First().enabled = isVisible;
    }

    public static void SetMaskView(this HauntedMaskItem mask, int? maskView)
    {
        // Register original mesh for each mash type to reset with later
        if (!_originalMeshMap.ContainsKey(mask.maskTypeId))
        {
            _originalMeshMap.Add(mask.maskTypeId,
                mask.gameObject.GetComponentsInChildren<MeshFilter>().First(IsMainMaskMesh).mesh);
        }
        // Register original material for each mash type to reset with later
        if (!_originalMatMap.ContainsKey(mask.maskTypeId))
        {
            _originalMatMap.Add(mask.maskTypeId,
                mask.gameObject.GetComponentsInChildren<MeshRenderer>().First(IsMainMaskMesh).material);
        }

        // Set main mesh filter's mesh and renderer material
        var maskMeshFilter = mask.gameObject.GetComponentsInChildren<MeshFilter>().First(IsMainMaskMesh);
        maskMeshFilter.mesh = maskView switch
        {
            (int)HeldMaskView.Outline => OutlineMesh,
            _ => _originalMeshMap[mask.maskTypeId]
        };
        var maskMeshRenderer = mask.gameObject.GetComponentsInChildren<MeshRenderer>().First(IsMainMaskMesh);
        maskMeshRenderer.material = maskView is (int)HeldMaskView.Translucent
            ? TransparentMat
            : _originalMatMap[mask.maskTypeId];
    }

    private static bool IsMainMaskMesh(Component component) => !(component.name is "EyesFilled" || component.name.StartsWith("ScanNode"));

    public static void SetMaskAttached(this HauntedMaskItem mask, bool isAttaching)
    {
        // Early exit if already applied: isAttaching and active, or !isAttaching and !active
        if (!(isAttaching ^ mask.currentHeadMask != null)) return;

        if (!isAttaching) NetworkHandler.Instance.MyPretend.IsMaskEyesOn = false;

        //Based on HauntedMaskItem.MaskClampToHeadAnimationEvent
        var player = AccessTools.Field(typeof(HauntedMaskItem), "previousPlayerHeldBy").GetValue(mask) as PlayerControllerB;

        if (isAttaching)
        {
            if (player != null)
            {
                mask.currentHeadMask = Object.Instantiate(mask.gameObject, null).transform;

                AdjustMaskOffsets(mask, player.IsLocal());

                AccessTools.Method(typeof(HauntedMaskItem), "PositionHeadMaskWithOffset").Invoke(mask, null);

                AccessTools.Field(typeof(HauntedMaskItem), "clampedToHead").SetValue(mask, true);

                // Set head mask's mask view only for the local player
                var headMask = mask.currentHeadMask.gameObject.GetComponent<HauntedMaskItem>();
                headMask.SetMaskView(player.IsLocal()
                    ? (int)Plugin.Config.AttachedMaskViewConfig.LocalValue
                    : null);
                headMask.enabled = false;
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

        // Set held mask visibility based on attach status
        if (player != null && player.IsLocal())
        {
            mask.SetMaskView(isAttaching
                ? (int)Plugin.Config.HeldMaskView.LocalValue
                : null);
        }
        mask.SetVisibility(!isAttaching);

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
        mask.headRotationOffset = _originalHeadMaskRot + (isLocal
            ? _localHeadMaskRotAdjustment
            : _publicHeadMaskRotAdjustment);
        headMask.transform.localScale = _originalHeadMaskScale * 0.83f;
    }

    public static void SetMaskEyes(this HauntedMaskItem mask, bool toActivate)
    {
        var eyesFilled = mask.currentHeadMask != null
            ? mask.currentHeadMask.gameObject.GetComponent<HauntedMaskItem>().maskEyesFilled
            : mask.maskEyesFilled;

        // Early exit if already applied: toActivate and enabled, or !toActive and !enabled
        if (!(toActivate ^ eyesFilled.enabled)) return;

        eyesFilled.enabled = toActivate;

        if (mask.playerHeldBy.IsLocal()) mask.SetControlTipsForItem();
    }
}
