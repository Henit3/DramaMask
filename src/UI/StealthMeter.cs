using DunGen;
using LCVR.Player;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DramaMask.UI;

// Adapted from the Oxygen mod: https://github.com/execOQ/Oxygen-LC/blob/master/General/OxygenInit.cs
public class StealthMeter : MonoBehaviour
{
    private static bool IsOxygenInstalled =>
        BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("consequential.Oxygen");
    private static bool IsEladsHudInstalled =>
        BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("me.eladnlg.customhud");
    private static bool IsShyHudInstalled =>
        BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("ShyHUD");
    private static bool IsLcVrInstalled =>
        BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("io.daxcess.lcvr");

    private const float AccurateMinValue = 0.2978f;
    private const float AccurateMaxValue = 0.9101f;
    private const float AccurateValueRange = AccurateMaxValue - AccurateMinValue;

    private static float AdjustFillAmount(float value) => Plugin.Config.AccurateMeter
        ? (value * AccurateValueRange) + AccurateMinValue
        : value;
    private static float InvAdjustFillAmount(float value) => Plugin.Config.AccurateMeter
        ? (value - AccurateMinValue) / AccurateValueRange
        : value;

    private static GameObject WeightUi
        => GameObject.Find("Systems/UI/Canvas/IngamePlayerHUD/TopLeftCorner/WeightUI");
    private static GameObject StatusEffectContainer
        => GameObject.Find("Systems/UI/Canvas/IngamePlayerHUD/TopLeftCorner/StatusEffects");

    private static Vector3 _initWeightPosition;
    private static Vector3 _initStatusPosition;

    private static int VrArmHudScaler => LCVR.Plugin.Config.DisableArmHUD.Value ? 1 : 2;

    // Workaround: Can use Instance, but comparisons show Instance == null as true
    public static bool Initialised = false;
    public static StealthMeter Instance { get; private set; }

    public bool Visible
    {
        get
        {
            if (uiElement == null) return false;
            return uiElement.enabled;
        }
        set
        {
            if (uiElement == null) return;
            uiElement.enabled = value;

            if (uiEladsText != null) uiEladsText.enabled = value;
        }
    }

    public float Percent
    {
        get
        {
            if (uiElement == null) return 0f;
            return IsEladsHudInstalled ? uiElement.fillAmount : InvAdjustFillAmount(uiElement.fillAmount);
        }
        set
        {
            if (uiElement == null) return;

            var adjustedFillAmount = IsEladsHudInstalled ? value : AdjustFillAmount(value);
            if (uiElement.fillAmount == adjustedFillAmount) return;
            uiElement.fillAmount = adjustedFillAmount;

            if (uiEladsText != null)
            {
                float roundedValue = (float)Math.Round(Percent, 2);
                int oxygenInPercent = (int)(roundedValue * 100);

                uiEladsText.text = $"{oxygenInPercent}<size=75%><voffset=1>%</voffset></size>";
            }

            if (IsShyHudInstalled)
            {
                var toFadeOut = value >= 0.75f;
                uiElement.CrossFadeAlpha(toFadeOut ? 0f : 1f, toFadeOut ? 5f : 0.5f, ignoreTimeScale: false);
            }
        }
    }

    public Color Colour
    {
        get
        {
            if (uiElement == null) return Color.white;
            return uiElement.color;
        }
        set
        {
            if (uiElement == null) return;
            uiElement.color = value;
            if (uiEladsText != null) uiEladsText.color = value;
        }
    }

    private GameObject _sprintMeter;
    private GameObject SprintMeter
    {
        get
        {
            if (_sprintMeter == null)
            {
                _sprintMeter = IsEladsHudInstalled
                    ? GameObject.Find("Systems/UI/Canvas/IngamePlayerHUD/PlayerInfo(Clone)/Stamina")
                    : GameNetworkManager.Instance.localPlayerController.sprintMeterUI.gameObject;
                if (_sprintMeter == null) Plugin.Logger.LogError("SprintMeter is null");
            }
            return _sprintMeter;
        }
    }
    
    private GameObject _stealthMeter;
    private Image uiElement;
    private TextMeshProUGUI uiEladsText;

    public static void Init()
    {
        if (Initialised)
        {
            Plugin.Logger.LogDebug("StealthMeter already initialized");
            return;
        }
        
        Instance = new();
        Initialised = true;
    }

    private static void SetInitPositions()
    {
        _initWeightPosition = WeightUi == null
            ? default
            : WeightUi.transform.localPosition;

        _initStatusPosition = StatusEffectContainer == null
            ? default
            : StatusEffectContainer.transform.localPosition;
    }

    private StealthMeter()
    {
        var topLeftCorner = SprintMeter.transform.parent.gameObject;

        _stealthMeter = Instantiate(SprintMeter, topLeftCorner.transform);
        _stealthMeter.name = "StealthMeter";

        if (IsLcVrInstalled)
        {
            CoroutineHelper.Start(InitVr());
            return;
        }

        SetInitPositions();
        
        if (IsEladsHudInstalled) InitEladsHud();
        else InitVanilla();

        Colour = Plugin.Config.MeterColour;
        Percent = 1f;
    }

    private void InitVanilla()
    {
        uiElement = _stealthMeter.transform.GetComponent<Image>();

        ApplyMeterOffsets();

        Plugin.Logger.LogInfo("StealthMeter initialised (Vanilla)");
    }

    public void ApplyMeterOffsets()
    {
        if (IsLcVrInstalled) ApplyVrMeterOffsets();
        else ApplyMeterOffsetsCommon();
    }

    private void ApplyMeterOffsetsCommon()
    {
        if (_stealthMeter == null)
        {
            Plugin.Logger.LogError("StealthMeter was uninitialised while applying offsets");
            return;
        }

        var sprintRectTransform = SprintMeter.GetComponent<RectTransform>();
        if (sprintRectTransform == null)
        {
            Plugin.Logger.LogError("SprintRectTransform not found while applying offsets");
            return;
        }

        // LCVR: Re-using same code with x and z components switched
        //       Adjust position of meter & self when using arm HUD
        var armHudScaler = 1;
        try { if (IsLcVrInstalled) armHudScaler = VrArmHudScaler; } catch { }
        var offsetInterval = IsLcVrInstalled
            ? new Vector3(0, -5f, 3f) / armHudScaler
            : new Vector3(3f, -5f, 0);
        var scaleInterval = IsLcVrInstalled
            ? new Vector3(0, 0.4f, 0.3f)
            : new Vector3(0.3f, 0.4f, 0);

        _stealthMeter.transform.localPosition = sprintRectTransform.localPosition
            + Plugin.Config.MeterOffset * offsetInterval;
        _stealthMeter.transform.localScale = sprintRectTransform.localScale
            + Plugin.Config.MeterOffset * scaleInterval;
        _stealthMeter.transform.rotation = sprintRectTransform.rotation;
        
        if (StatusEffectContainer != null)
        {
            // 20 per meter, cap meter at 1 if oxygen due to oxygen bar
            var offsetCap = IsOxygenInstalled && !IsLcVrInstalled ? 1 : 0;
            StatusEffectContainer.transform.localPosition = _initStatusPosition
                + new Vector3(Math.Max(offsetCap, Plugin.Config.MeterOffset) * 20, 0, 0);

            Plugin.Logger.LogInfo("StatusEffectHUD adjusted");
        }

        if (WeightUi != null)
        {
            // 20 per meter, cap total at 0 due to vanilla stamina bar
            WeightUi.transform.localPosition = _initWeightPosition
                + new Vector3(Math.Max(0, Plugin.Config.MeterOffset * 20), 0, 0);

            Plugin.Logger.LogInfo("WeightUI adjusted");
        }
    }

    private void InitEladsHud()
    {
        uiElement = _stealthMeter.transform.Find("Bar/StaminaBar").GetComponent<Image>();
        uiEladsText = _stealthMeter.transform.Find("StaminaInfo").GetComponent<TextMeshProUGUI>();
        uiEladsText.horizontalAlignment = HorizontalAlignmentOptions.Right;

        // Destroy children objects that aren't needed
        DestroyImmediate(_stealthMeter.transform.Find("CarryInfo").gameObject);
        DestroyImmediate(_stealthMeter.transform.Find("Bar/Stamina Change FG").gameObject);

        _stealthMeter.transform.localPosition = SprintMeter.transform.localPosition
            + new Vector3(0f, 60f, 0);

        Plugin.Logger.LogInfo("StealthMeter initialised (EladsHUD)");
    }

    // Adapted from the InsanityDisplay mod: https://github.com/Confusified/LC-InsanityDisplay/blob/master/ModCompatibility/LethalCompanyVRCompatibility.cs
    private IEnumerator InitVr()
    {
        if (!VRSession.InVR) yield break;

        uiElement = _stealthMeter.transform.GetComponent<Image>();

        // Set parent to sprint meter until VR Instance exists
        _stealthMeter.transform.SetParent(SprintMeter.transform, false);

        // Wait until VR Instance exists
        yield return new WaitUntil(() => VRSession.Instance != null);

        SetInitPositions();

        ApplyVrMeterOffsets();

        Colour = Plugin.Config.MeterColour;
        Percent = 1f;

        Plugin.Logger.LogInfo("StealthMeter initialised (VR)");
    }

    private void ApplyVrMeterOffsets()
    {
        if (_stealthMeter == null)
        {
            Plugin.Logger.LogError("StealthMeter was uninitialised while applying offsets");
            return;
        }
        
        // Set parent to TopLeftCorner equivalent for positioning
        Transform sprintMeterTransform = SprintMeter.transform;
        _stealthMeter.transform.SetParent(sprintMeterTransform.parent, false);

        ApplyMeterOffsets();

        // InsanityDisplay: Shouldn't have any negative effects(?) and will hide when LCVR's HUD hides
        _stealthMeter.transform.SetParent(sprintMeterTransform, true);
    }
}