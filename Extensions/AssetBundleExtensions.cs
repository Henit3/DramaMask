using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DramaMask.Extensions;

public static class AssetBundleExtensions
{
    public static T SafeLoadAsset<T>(this AssetBundle bundle, string assetName) where T : Object
    {
        if (!(bundle?.GetAllAssetNames().Contains(assetName) ?? false))
        {
            Plugin.Logger.LogError($"Asset [{assetName}] not found in bundle [{bundle.name}]!");
            return default;
        }
        return bundle.LoadAsset<T>(assetName);
    }
}
