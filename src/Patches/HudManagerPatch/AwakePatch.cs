using DramaMask.Constants;
using DramaMask.UI;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace DramaMask.Patches.HudManagerPatch;

[HarmonyPatch(typeof(HUDManager), "Awake")]
public class AwakePatch
{
    private static Vector2 _barSize = new(150f, 10f);
    private static Vector3 _barPosition;

    [HarmonyPostfix]
    public static void Postfix(HUDManager __instance, ref HUDElement[] ___HUDElements)
    {
        if (!Plugin.Config.UseStealthMeter.Value
            || Plugin.Config.StealthMeterVisibility.LocalValue is MeterVisibility.Never) return;

        _barPosition = new(Plugin.Config.BarXPosition, Plugin.Config.BarYPosition, -0.075f);

        Transform parent = ___HUDElements[2].canvasGroup.transform.parent;

        var stealthMeter = new GameObject("StealthMeter",
        [
            typeof(Image),
            typeof(CanvasGroup)
        ]);
        ___HUDElements = ___HUDElements.AddToArray(new HUDElement
        {
            canvasGroup = stealthMeter.GetComponent<CanvasGroup>(),
            targetAlpha = 1f
        });

        var meterPos = stealthMeter.GetComponent<RectTransform>();
        meterPos.pivot = Vector2.one * 0.5f;
        meterPos.sizeDelta = _barSize;
        meterPos.anchoredPosition3D = _barPosition;

        var barBackground = stealthMeter.GetComponent<Image>();
        barBackground.sprite = CreateSpriteFromTexture(Texture2D.whiteTexture);
        barBackground.color = Color.black;
        barBackground.transform.SetParent(parent, false);

        var stealthBar = new GameObject("StealthBar",
        [
            typeof(Image),
            typeof(StealthMeterUI)
        ]);
        stealthBar.transform.SetParent(barBackground.transform, false);

        var barPos = stealthBar.GetComponent<RectTransform>();
        barPos.pivot = Vector2.one * 0.5f;
        barPos.sizeDelta = _barSize;

        var barForeground = stealthBar.GetComponent<Image>();
        barForeground.fillMethod = 0;
        barForeground.type = (Image.Type)3;
        barForeground.sprite = CreateSpriteFromTexture(Texture2D.whiteTexture);
        barForeground.color = Plugin.Config.BarColour;

        StealthMeterUI.Instance.RelatedVisuals = [
            barBackground,
            barForeground
        ];
        StealthMeterUI.Instance.Visible = Plugin.Config.StealthMeterVisibility.LocalValue is MeterVisibility.Always;
    }

    private static Sprite CreateSpriteFromTexture(Texture2D texture2D, float? width = null)
    {
        Sprite val = Sprite.Create(texture2D,
            new Rect(0f, 0f, width ?? texture2D.width, texture2D.height),
            new Vector2(0.5f, 0.5f));
        val.name = texture2D.name;
        return val;
    }
}