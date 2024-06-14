using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;

namespace DramaMask.Extensions;

public static class HarmonyExtensions
{
    public static void PatchExcept(this Harmony harmony, Type[] exceptions)
    {
        var allPatchTypes = Assembly.GetExecutingAssembly().GetTypes();
        foreach (var patchType in allPatchTypes.Except(exceptions))
        {
            harmony.PatchAll(patchType);
        }
    }
}
