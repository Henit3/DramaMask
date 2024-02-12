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

    public static void SetMaskAttached(this HauntedMaskItem mask, bool isAttaching/*,
        bool ___attaching, Transform ___currentHeadMask, GameObject ___headMaskPrefab,
        PlayerControllerB ___previousPlayerHeldBy, bool ___clampedToHead*/)
    {
        // TODO: Don't want to destroy in hand actually, just hide (maybe replicate or transpile in condition)
        // TODO: Don't want to destroy headMask either, just disable/enable it if not null
        // Cannot access privates directly here since not a patch
        if (!isAttaching) mask.SetMaskEyes(false);

        /*if (isAttaching)
        {
            ___attaching = true;
            mask.MaskClampToHeadAnimationEvent();
           
        }
        else
        {
            Debug.Log("Mask declamp animation event called");
            if (!(___previousPlayerHeldBy == null))
            {
                Debug.Log("Destroying currentHeadMask");
                MeshRenderer[] componentsInChildren = ___headMaskPrefab.gameObject.GetComponentsInChildren<MeshRenderer>();
                for (int i = 0; i < componentsInChildren.Length; i++)
                {
                    Object.Destroy(componentsInChildren[i]);
                }
                ___currentHeadMask = null;
                ___previousPlayerHeldBy.playerBodyAnimator.SetBool("HoldMask", value: false);
                Debug.Log($"Instantiating object in hand; headmask null: {___currentHeadMask == null}");
                ___clampedToHead = true;
            }
        }*/
    }

    public static void SetMaskEyes(this HauntedMaskItem mask, bool toActivate)
    {
        mask.maskEyesFilled.enabled = toActivate;
    }
}
