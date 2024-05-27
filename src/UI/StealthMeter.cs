using UnityEngine;
using UnityEngine.UI;

namespace DramaMask.UI;

// Adapted from the Oxygen mod: https://github.com/execOQ/Oxygen-LC/blob/master/General/OxygenInit.cs
public class StealthMeter : MonoBehaviour
{
    private const float AccurateMinValue = 0.2978f;
    private const float AccurateMaxValue = 0.9101f;
    private const float AccurateValueRange = AccurateMaxValue - AccurateMinValue;

    private static float AdjustFillAmount(float value) => Plugin.Config.AccurateMeter
        ? (value * AccurateValueRange) + AccurateMinValue
        : value;
    private static float InvAdjustFillAmount(float value) => Plugin.Config.AccurateMeter
        ? (value - AccurateMinValue) / AccurateValueRange
        : value;

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

        Instance = new();
        Plugin.Logger.LogInfo("StealthMeter initialized");
    }

    private StealthMeter()
    {
        InitVanilla();
    }

    private void InitVanilla()
    {
        GameObject topLeftCorner = GameObject.Find("Systems/UI/Canvas/IngamePlayerHUD/TopLeftCorner");
        if (topLeftCorner == null)
        {
            Plugin.Logger.LogError("TopLeftCorner not found");
            return;
        }

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

        RectTransform sprintRectTransform = SprintMeter.GetComponent<RectTransform>();
        if (sprintRectTransform == null)
        {
            Plugin.Logger.LogError("SprintRectTransform not found while applying offsets");
            return;
        }

        RectTransform rectTransform = _stealthMeter.GetComponent<RectTransform>();
        rectTransform.anchorMin = sprintRectTransform.anchorMin;
        rectTransform.anchorMax = sprintRectTransform.anchorMax;
        rectTransform.pivot = sprintRectTransform.pivot;

        var offsetPosX = Plugin.Config.MeterOffset * 3f;
        var offsetPosY = Plugin.Config.MeterOffset * -5f;
        var offsetScaleX = Plugin.Config.MeterOffset * 0.3f;
        var offsetScaleY = Plugin.Config.MeterOffset * 0.4f;

        rectTransform.localPosition = sprintRectTransform.localPosition + new Vector3(offsetPosX, offsetPosY, 0);
        rectTransform.localScale = sprintRectTransform.localScale + new Vector3(offsetScaleX, offsetScaleY, 0);
        rectTransform.rotation = sprintRectTransform.rotation;

        // TODO: Status effect positioning
        // TODO: Weight display positioning
    }
}