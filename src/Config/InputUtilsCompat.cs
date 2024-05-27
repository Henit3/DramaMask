using UnityEngine.InputSystem;

namespace DramaMask.Config;

internal static class InputUtilsCompat
{
    private static bool Installed =>
        BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.rune580.LethalCompanyInputUtils");
    public static bool Enabled => Installed && !Plugin.Config.IgnoreCustomKeybinds;

    public static void Init()
    {
        if (Installed) InputUtilsConfig.Instance = new();
    }

    public static InputAction AttachMask => InputUtilsConfig.Instance.AttachMask;
    public static bool HandleAttachMask;

    public static InputAction MaskEyes => InputUtilsConfig.Instance.MaskEyes;
    public static bool HandleMaskEyes;

    public static bool IsMaskAttachDefaultClash() => InputUtilsConfig.ClashesWithAction(AttachMask, "ItemSecondaryUse");
    public static bool IsMaskEyeInteractClash() => InputUtilsConfig.ClashesWithAction(MaskEyes, "Interact");
    public static bool IsMaskEyeDefaultClash() => InputUtilsConfig.ClashesWithAction(MaskEyes, "ItemTertiaryUse");
}