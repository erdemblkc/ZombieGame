using UnityEngine;

/// <summary>
/// Momentum Surge — passive upgrade. After every kill, gain a 1.5× speed
/// boost for 3 seconds. Timer resets on each kill; boosts do not stack.
/// Listens to GameEvents.OnEnemyKilled.
/// </summary>
public class MomentumSurgeUpgrade : MovementUpgradeBase
{
    [Header("Momentum Surge Parameters")]
    [SerializeField] private float _speedMultiplier = 1.5f;
    [SerializeField] private float _duration        = 3f;

    private float _boostTimer;
    private bool  _wasActive;

    public bool  IsBoosting       => _boostTimer > 0f;
    public float BoostTimeRemaining => Mathf.Max(0f, _boostTimer);

    public override void OnUpgradeEnabled(GameObject player)
    {
        base.OnUpgradeEnabled(player);
        GameEvents.OnEnemyKilled += HandleKill;
    }

    public override void OnUpgradeDisabled()
    {
        GameEvents.OnEnemyKilled -= HandleKill;
        // Reset speed if boost was active
        if (_mods != null && _wasActive)
            _mods.SpeedMultiplier = 1f;
        base.OnUpgradeDisabled();
    }

    void Update()
    {
        if (!IsRunnable)
        {
            if (_wasActive && _mods != null)
            {
                _mods.SpeedMultiplier = 1f;
                _wasActive = false;
            }
            return;
        }

        if (_boostTimer > 0f)
        {
            _boostTimer -= Time.deltaTime;
            _mods.SpeedMultiplier = _speedMultiplier;
            _wasActive = true;
        }
        else if (_wasActive)
        {
            _mods.SpeedMultiplier = 1f;
            _wasActive = false;
        }
    }

    void HandleKill(GameObject _)
    {
        if (!IsRunnable) return;
        // Reset (not add) the timer so boosts don't stack
        _boostTimer = _duration;
    }
}
