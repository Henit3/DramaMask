﻿namespace DramaMask.Network.Models;

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
            if (ShouldCopyToMap()) NetworkHandler.Instance.PretendMap[PlayerId].IsMaskAttached = value;
            if (ShouldServerProcess()) NetworkHandler.Instance.SetPlayerMaskAttachedServer(PlayerId, value);
        }
    }

    private bool _isRaisingArms = false;
    public bool IsRaisingArms
    {
        get => _isRaisingArms;
        set
        {
            _isRaisingArms = value;
            if (ShouldCopyToMap()) NetworkHandler.Instance.PretendMap[PlayerId].IsRaisingArms = value;
            if (ShouldServerProcess()) NetworkHandler.Instance.SetPlayerRaisingArmsServer(PlayerId, value);
        }
    }

    private bool _isMaskEyesOn = false;
    public bool IsMaskEyesOn
    {
        get => _isMaskEyesOn;
        set
        {
            _isMaskEyesOn = value;
            if (ShouldCopyToMap()) NetworkHandler.Instance.PretendMap[PlayerId].IsMaskEyesOn = value;
            if (ShouldServerProcess()) NetworkHandler.Instance.SetPlayerMaskEyesServer(PlayerId, value);
        }
    }

    public override void Reset()
    {
        _isMaskAttached = false;
        _isRaisingArms = false;
        _isMaskEyesOn = false;
    }

    public override string ToString() => $"{base.ToString()}: {IsMaskAttached}|{IsRaisingArms}|{IsMaskEyesOn}";
}
