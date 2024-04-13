namespace DramaMask.UI;

public class StealthMeterUI : FillableMeterUI
{
    public static StealthMeterUI Instance { get; private set; }

    private new void Awake()
    {
        if (Instance == null) Instance = this;
        base.Awake();
        Name = "StealthBar";
    }
}