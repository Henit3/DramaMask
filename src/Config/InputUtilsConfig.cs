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
        => OnMaskAction(context, "ItemSecondaryUse", ref InputUtilsCompat.HandleAttachMask);

    public void OnMaskEyes(InputAction.CallbackContext context)
        => OnMaskAction(context, "ItemTertiaryUse", ref InputUtilsCompat.HandleMaskEyes);

    private void OnMaskAction(InputAction.CallbackContext context,
        string actionName, ref bool shouldHandleCustom)
    {
        if (!context.performed) return;

        shouldHandleCustom = true;

        if (IsDefaultBinding(context, actionName)) return;

        // Need to invoke manually if non-default bindings are in use
        AccessTools.Method(typeof(PlayerControllerB), $"{actionName}_performed")
            .Invoke(StartOfRound.Instance.localPlayerController, [context]);
    }

    private static bool IsDefaultBinding(InputAction.CallbackContext context, string actionName)
    {
        var action = IngamePlayerSettings.Instance.playerInput.actions
            .FindAction(actionName, throwIfNotFound: false);
        if (action == null) return false;

        var defaultControl = action.controls
            .FirstOrDefault(a => a.device == context.control.device);
        if (defaultControl == null) return false;

        return defaultControl.path == context.control.path;
    }

    public static bool ClashesWithAction(InputAction inputAction, string targetAction)
    {
        var action = IngamePlayerSettings.Instance.playerInput.actions
            .FindAction(targetAction, throwIfNotFound: false);
        if (action == null) return true;
        
        var interactControl = action.controls
            .FirstOrDefault(a => a.device.name == "Keyboard" ^ StartOfRound.Instance.localPlayerUsingController);
        if (interactControl == null) return false;

        var inputControl = inputAction.controls
            .FirstOrDefault(a => a.device.name == "Keyboard" ^ StartOfRound.Instance.localPlayerUsingController);
        if (inputControl == null) return false;

        return interactControl.path == inputControl.path;
    }
}