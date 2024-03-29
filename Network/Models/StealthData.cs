using System;

namespace DramaMask.Network.Models;

public class StealthData : NetworkData
{
    public StealthData() : base() { }
    public StealthData(ulong playerId) : base(playerId) { }

    private bool _isHoldingMask = false;
    public bool IsHoldingMask
    {
        get => _isHoldingMask;
        set
        {
            if (_isHoldingMask == value) return;
            _isHoldingMask = value;
            if (ShouldCopyToMap()) NetworkHandler.Instance.StealthMap[PlayerId].IsHoldingMask = value;

            HandleToggleHidden(_isHoldingMask);
        }
    }

    public bool IsAttemptingStealth()
    {
        var pretendData = IsClientCopy
            ? NetworkHandler.Instance.MyPretend
            : NetworkHandler.Instance.PretendMap[PlayerId];
        return IsHoldingMask || pretendData.IsMaskAttached;
    }

    private float _stealthValue = ConfigValues.MaxHiddenTime;
    private bool _isStealthValueValid = true;
    public float StealthValue
    {
        get => _stealthValue;
        set
        {
            var previousValue = _stealthValue;
            _stealthValue = value;
            if (ShouldCopyToMap()) NetworkHandler.Instance.StealthMap[PlayerId].StealthValue = value;

            if (previousValue <= 0 && value > 0) _isStealthValueValid = true;
            else if (previousValue > 0 && value <= 0)
            {
                _isStealthValueValid = false;
                AddExhaustionPenalty = true;
            }
            else return;

            HandleToggleHidden(_isStealthValueValid);
        }
    }

    private DateTime? _lastStoppedStealth = null;
    public DateTime? LastStoppedStealth
    {
        get => _lastStoppedStealth;
        set
        {
            _lastStoppedStealth = value;
            if (ShouldCopyToMap()) NetworkHandler.Instance.StealthMap[PlayerId].LastStoppedStealth = value;
        }
    }

    private bool _addExhaustionPenalty = false;
    public bool AddExhaustionPenalty
    {
        get => _addExhaustionPenalty;
        set
        {
            _addExhaustionPenalty = value;
            if (ShouldCopyToMap()) NetworkHandler.Instance.StealthMap[PlayerId].AddExhaustionPenalty = value;
        }
    }

    public void HandleToggleHidden(bool isHiddenProposed)
    {
        if (!ShouldServerProcess()) return;

        var isAttemptingStealth = IsAttemptingStealth();
        if (isHiddenProposed
            && (isAttemptingStealth && _isStealthValueValid)
            && NetworkHandler.Instance.VisiblePlayers.Contains(PlayerId)) { /* Toggled to true and is now true */ }
        else if (!isHiddenProposed
            && !(isAttemptingStealth && _isStealthValueValid)
            && !NetworkHandler.Instance.VisiblePlayers.Contains(PlayerId)) { /* Toggled to false and is now false */ }
        else return;

        NetworkHandler.Instance.SetPlayerHiddenServer(PlayerId, isHiddenProposed);
    }

    public void SetLastStoppedStealthNow()
    {
        var adjustedTime = DateTime.UtcNow;
        if (AddExhaustionPenalty) adjustedTime = adjustedTime.AddSeconds(ConfigValues.ExhaustionPenaltyDelay);
        LastStoppedStealth = adjustedTime;
    }

    public override void Reset()
    {
        _isHoldingMask = false;
        _stealthValue = ConfigValues.MaxHiddenTime;
        _lastStoppedStealth = null;
        _addExhaustionPenalty = false;
    }

    public override string ToString() => $"{base.ToString()}: {IsHoldingMask}|{StealthValue}|{LastStoppedStealth}|{AddExhaustionPenalty}";
}
