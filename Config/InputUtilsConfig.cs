using DramaMask.Extensions;
using GameNetcodeStuff;
using HarmonyLib;
using LethalCompanyInputUtils.Api;
using System.Linq;
using UnityEngine.InputSystem;

namespace DramaMask.Config;

public class InputUtilsConfig : LcInputActions
{
    public static InputUtilsConfig Instance;

    [InputAction("<Keyboard>/q", Name = "Attach Mask", GamepadPath = "<Gamepad>/dpad/down")]
    public InputAction AttachMask { get; set; }
    [InputAction("<Keyboard>/e", Name = "Mask Eyes", GamepadPath = "<Gamepad>/dpad/up")]
    public InputAction MaskEyes { get; set; }

    public InputUtilsConfig()
    {
        AttachMask.performed += OnAttachMask;
        MaskEyes.performed += OnMaskEyes;
        DramaMask.Plugin.Logger.LogDebug("Loaded InputUtils for DramaMask");
    }

    public void OnAttachMask(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        var localPlayer = StartOfRound.Instance.allPlayerScripts.FirstOrDefault(player => player.IsLocal());
        if (localPlayer == null) return;

        InputUtilsCompat.HandleAttachMask = true;
        AccessTools.Method(typeof(PlayerControllerB), "ItemSecondaryUse_performed").Invoke(localPlayer, [context]);
    }

    public void OnMaskEyes(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        var localPlayer = StartOfRound.Instance.allPlayerScripts.FirstOrDefault(player => player.IsLocal());
        if (localPlayer == null) return;

        InputUtilsCompat.HandleMaskEyes = true;
        AccessTools.Method(typeof(PlayerControllerB), "ItemTertiaryUse_performed").Invoke(localPlayer, [context]);
    }
}