using System;
using UnityEngine;
using UnityEngine.UI;

namespace DramaMask.UI;

// Adapted from the Oxygen mod: https://github.com/execOQ/Oxygen-LC/blob/master/General/OxygenInit.cs
public class StealthMeter : MonoBehaviour
{
    private static bool IsOxygenInstalled =>
        BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("consequential.Oxygen");

    private const float AccurateMinValue = 0.2978f;
    private const float AccurateMaxValue = 0.9101f;
    private const float AccurateValueRange = AccurateMaxValue - AccurateMinValue;

    private static float AdjustFillAmount(float value) => Plugin.Config.AccurateMeter
        ? (value * AccurateValueRange) + AccurateMinValue
        : value;
    private static float InvAdjustFillAmount(float value) => Plugin.Config.AccurateMeter
        ? (value - AccurateMinValue) / AccurateValueRange
        : value;

    private static GameObject _weightUi
        => GameObject.Find("Systems/UI/Canvas/IngamePlayerHUD/TopLeftCorner/WeightUI");
    private static GameObject _statusEffectContainer
        => GameObject.Find("Systems/UI/Canvas/IngamePlayerHUD/TopLeftCorner/StatusEffects");

    private static Vector3 _initWeightPosition;
    private static Vector3 _initStatusPosition;

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
        }
    }

    public float Percent
    {
        get
        {
            if (uiElement == null) return 0f;
            return InvAdjustFillAmount(uiElement.fillAmount);
        }
        set
        {
            var adjustedFillAmount = AdjustFillAmount(value);
            if (uiElement.fillAmount == adjustedFillAmount) return;
            uiElement.fillAmount = adjustedFillAmount;
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
            uiElement.color = value;
        }
    }

    private GameObject _sprintMeter;
    private GameObject SprintMeter
    {
        get
        {
            if (_sprintMeter == null)
            {
                _sprintMeter = GameObject.Find("Systems/UI/Canvas/IngamePlayerHUD/TopLeftCorner/SprintMeter");
                if (_sprintMeter == null) Plugin.Logger.LogError("SprintMeter is null");
            }
            return _sprintMeter;
        }
    }
    
    private GameObject _stealthMeter;
    private Image uiElement;

    public static void Init()
    {
        if (Instance != null)
        {
            Plugin.Logger.LogDebug("StealthMeter already initialized");
            return;
        }

        SetInitPositions();

        Instance = new();
        Plugin.Logger.LogInfo("StealthMeter initialized");
    }

    private static void SetInitPositions()
    {
        _initWeightPosition = _weightUi == null
            ? new Vector3(0, 0, 0)
            : _weightUi.transform.localPosition;

        _initStatusPosition = _statusEffectContainer == null
            ? new Vector3(0, 0, 0)
            : _statusEffectContainer.transform.localPosition;
    }

    private StealthMeter()
    {
        InitVanilla();
    }

    private void InitVanilla()
    {
        var topLeftCorner = SprintMeter.transform.parent.gameObject;

        _stealthMeter = Instantiate(SprintMeter, topLeftCorner.transform);
        _stealthMeter.name = "StealthMeter";
        uiElement = _stealthMeter.transform.GetComponent<Image>();
        uiElement.color = Plugin.Config.MeterColour;
        Percent = 1f;

        ApplyMeterOffsets();
    }

    public void ApplyMeterOffsets()
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

        var rectTransform = _stealthMeter.GetComponent<RectTransform>();
        rectTransform.anchorMin = sprintRectTransform.anchorMin;
        rectTransform.anchorMax = sprintRectTransform.anchorMax;
        rectTransform.pivot = sprintRectTransform.pivot;

        rectTransform.localPosition = sprintRectTransform.localPosition
            + Plugin.Config.MeterOffset * new Vector3(3f, -5f, 0);
        rectTransform.localScale = sprintRectTransform.localScale
            + Plugin.Config.MeterOffset * new Vector3(0.3f, 0.4f, 0);
        rectTransform.rotation = sprintRectTransform.rotation;
        
        if (_statusEffectContainer != null)
        {
            // 20 per meter, cap meter at 1 if oxygen due to oxygen bar
            var offsetCap = IsOxygenInstalled ? 1 : 0;
            _statusEffectContainer.transform.localPosition = _initStatusPosition
                + new Vector3(Math.Min(offsetCap, Plugin.Config.MeterOffset) * 20, 0, 0);

            Plugin.Logger.LogInfo("StatusEffectHUD adjusted");
        }

        if (_weightUi != null)
        {
            // 20 per meter, cap total at 0 due to vanilla stamina bar
            _weightUi.transform.localPosition = _initWeightPosition
                + new Vector3(Math.Min(0, Plugin.Config.MeterOffset * 20), 0, 0);

            Plugin.Logger.LogInfo("WeightUI adjusted");
        }
    }
}