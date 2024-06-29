using DramaMask.Network;

namespace DramaMask.Models.Network;

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
            if (_isMaskAttached == value) return;

            _isMaskAttached = value;
            if (ShouldCopyToMap()) NetworkHandler.Instance.PretendMap[PlayerId].IsMaskAttached = value;
            if (ShouldServerProcess()) NetworkHandler.Instance.SetPlayerMaskAttachedServer(PlayerId, value);

            var stealthData = NetworkHandler.Instance.GetStealth(IsLocalCopy, PlayerId);
            if (stealthData == null) return;

            stealthData.HandleToggleHidden(_isMaskAttached);
        }
    }

    private bool _isMaskEyesOn = false;
    public bool IsMaskEyesOn
    {
        get => _isMaskEyesOn;
        set
        {
            if (_isMaskEyesOn == value) return;

            _isMaskEyesOn = value;
            if (ShouldCopyToMap()) NetworkHandler.Instance.PretendMap[PlayerId].IsMaskEyesOn = value;
            if (ShouldServerProcess()) NetworkHandler.Instance.SetPlayerMaskEyesServer(PlayerId, value);
        }
    }

    public override void Reset()
    {
        _isMaskAttached = false;
        _isMaskEyesOn = false;
    }

    public override string ToString() => $"{base.ToString()}: {IsMaskAttached}|{IsMaskEyesOn}";
}
