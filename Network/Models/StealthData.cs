using System;

namespace DramaMask.Network.Models;

public class StealthData : NetworkData
{
    public StealthData() : base() { }
    public StealthData(ulong playerId) : base(playerId) { }

    private bool _isAttemptingStealth = false;
    public bool IsAttemptingStealth
    {
        get => _isAttemptingStealth;
        set
        {
            if (_isAttemptingStealth == value) return;
            _isAttemptingStealth = value;
            if (ShouldCopyToMap()) NetworkHandler.Instance.StealthMap[PlayerId].IsAttemptingStealth = value;

            HandleToggleHidden(_isAttemptingStealth);
        }
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

    private void HandleToggleHidden(bool isHiddenProposed)
    {
        if (!ShouldServerProcess()) return;

        if (isHiddenProposed
            && (IsAttemptingStealth && _isStealthValueValid)
            && NetworkHandler.Instance.VisiblePlayers.Contains(PlayerId)) { /* Toggled to true and is now true */ }
        else if (!isHiddenProposed
            && !(IsAttemptingStealth && _isStealthValueValid)
            && !NetworkHandler.Instance.VisiblePlayers.Contains(PlayerId)) { /* Toggled to false and is now false */ }
        else return;

        NetworkHandler.Instance.TogglePlayerHiddenServer(PlayerId);
    }

    public override void Reset()
    {
        _isAttemptingStealth = false;
        _stealthValue = ConfigValues.MaxHiddenTime;
        _lastStoppedStealth = null;
        _addExhaustionPenalty = false;
    }

    public override string ToString() => $"{base.ToString()}: {IsAttemptingStealth}|{StealthValue}|{LastStoppedStealth}|{AddExhaustionPenalty}";
}
