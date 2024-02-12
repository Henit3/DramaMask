namespace DramaMask.Network.Models;

public class PretendData : NetworkData
{
    public PretendData() : base() { }
    public PretendData(ulong playerId) : base(playerId) { }

    private bool _isMaskAttached = false;
    public bool IsMaskAttached
    {
        get => _isMaskAttached;
        set
        {
            _isMaskAttached = value;
            if (!ShouldCopyToMap()) return;

            NetworkHandler.Instance.PretendMap[PlayerId].IsMaskAttached = value;
            NetworkHandler.Instance.SetPlayerMaskAttachedServerRpc(PlayerId, value);
        }
    }

    private bool _isRaisingArms = false;
    public bool IsRaisingArms
    {
        get => _isRaisingArms;
        set
        {
            _isRaisingArms = value;
            if (!ShouldCopyToMap()) return;

            NetworkHandler.Instance.PretendMap[PlayerId].IsRaisingArms = value;
            NetworkHandler.Instance.SetPlayerRaisingArmsServerRpc(PlayerId, value);
        }
    }

    private bool _isMaskEyesOn = false;
    public bool IsMaskEyesOn
    {
        get => _isMaskEyesOn;
        set
        {
            _isMaskEyesOn = value;
            if (!ShouldCopyToMap()) return;

            NetworkHandler.Instance.PretendMap[PlayerId].IsMaskEyesOn = value;
            NetworkHandler.Instance.SetPlayerMaskEyesServerRpc(PlayerId, value);
        }
    }

    public override string ToString() => $"{base.ToString()}: {null}";
}
