using System;
using DramaMask.Network;

namespace DramaMask.Models.Network;

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
        var pretendData = IsLocalCopy
            ? NetworkHandler.Instance.MyPretend
            : NetworkHandler.Instance.PretendMap[PlayerId];
        return IsHoldingMask || pretendData.IsMaskAttached;
    }

    private float _stealthValue = Plugin.Config.MaxHiddenTime.Value;
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
        // If the proposed value is valid to action within the current scope (local/map): "If should hide + can hide"
        if (!(isHiddenProposed == (IsAttemptingStealth() && _isStealthValueValid))) return;

        // Don't update for host mapped copy value directly
        if (IsLocalCopy || PlayerId != 0)
        {
            if (!isHiddenProposed) SetLastStoppedStealthNow();
        }

        if (!ShouldServerProcess()) return;

        // If the proposed value is valid to action with the shared network variable: "+ not applied on network var"
        if (!(isHiddenProposed == NetworkHandler.Instance.VisiblePlayers.Contains(PlayerId))) return;

        NetworkHandler.Instance.SetPlayerHiddenServer(PlayerId, isHiddenProposed);
    }

    public void SetLastStoppedStealthNow()
    {
        var adjustedTime = DateTime.UtcNow;
        if (AddExhaustionPenalty) adjustedTime = adjustedTime.AddSeconds(Plugin.Config.ExhaustionPenaltyDelay.Value);
        LastStoppedStealth = adjustedTime;
    }

    public override void Reset()
    {
        _isHoldingMask = false;
        _stealthValue = Plugin.Config.MaxHiddenTime.Value;
        _lastStoppedStealth = null;
        _addExhaustionPenalty = false;
    }

    public override string ToString() => $"{base.ToString()}: {IsHoldingMask}|{StealthValue}|{LastStoppedStealth}|{AddExhaustionPenalty}";
}
