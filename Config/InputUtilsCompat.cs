using UnityEngine.InputSystem;

namespace DramaMask.Config;

internal static class InputUtilsCompat
{
    public static bool Enabled =>
        BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.rune580.LethalCompanyInputUtils");

    public static void Init()
    {
        if (Enabled) InputUtilsConfig.Instance = new();
    }

    public static InputAction AttachMask => InputUtilsConfig.Instance.AttachMask;
    public static bool HandleAttachMask;

    public static InputAction MaskEyes => InputUtilsConfig.Instance.MaskEyes;
    public static bool HandleMaskEyes;

    public static bool IsMaskEyeInteractClash() => InputUtilsConfig.ClashesWithInteract(MaskEyes);
}