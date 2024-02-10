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
}
