using UnityEngine;

namespace DramaMask;

public static class ConfigValues
{
    public static bool AllMasksHide = true;
    public static bool HideFromAllEnemies = false;

    public static bool UseStealthMeter = true;
    public static float ExhaustionPenaltyDelay = 3;
    public static float MaxHiddenTime = 15;
    public static float RechargeDelay = 3;

    public static bool SeeStealthMeter = true;
    public static bool AlwaysSeeStealthMeter = false;
    public static float BarXPosition = 0f;
    public static float BarYPosition = 235f;
    public static Color BarColour = new(220, 220, 220, byte.MaxValue);

    public static bool SeeWornMaskOutline = false;
}
