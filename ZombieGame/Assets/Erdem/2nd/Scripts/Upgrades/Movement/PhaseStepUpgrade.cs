using UnityEngine;

/// <summary>
/// Phase Step — short-range instant blink in the look direction (F key).
/// Raycast prevents teleporting into solid geometry.
/// Two independent charges with separate cooldowns.
/// </summary>
public class PhaseStepUpgrade : MovementUpgradeBase
{
    [Header("Phase Step Parameters")]
    [SerializeField] private float   _maxDistance   = 5f;
    [SerializeField] private int     _chargeCount   = 2;
    [SerializeField] private float   _chargeCooldown = 3f;
    [SerializeField] private KeyCode _phaseKey      = KeyCode.F;

    private int     _charges;
    private float[] _cooldownTimers;

    public int Charges    => _charges;
    public int MaxCharges => _chargeCount;

    // ── Evolution hook: GhostDouble reads this ─────────────────────────────
    public event System.Action<Vector3> OnPhaseStepPerformed; // param = original position

    public override void OnUpgradeEnabled(GameObject player)
    {
        base.OnUpgradeEnabled(player);
        _charges        = _chargeCount;
        _cooldownTimers = new float[_chargeCount];
    }

    public override void OnUpgradeDisabled()
    {
        base.OnUpgradeDisabled();
    }

    void Update()
    {
        if (!IsRunnable) return;

        TickCooldowns();

        if (Input.GetKeyDown(_phaseKey) && _charges > 0)
            PerformPhaseStep();
    }

    void PerformPhaseStep()
    {
        if (_cam == null) return;

        Vector3 origin    = transform.position;
        Vector3 direction = _cam.transform.forward;
        Vector3 dest      = origin + direction * _maxDistance;

        // Pull back destination if wall in the way
        if (Physics.Raycast(origin, direction, out RaycastHit hit, _maxDistance))
            dest = hit.point - direction * 0.3f;

        // Make sure we don't go underground
        dest.y = Mathf.Max(dest.y, origin.y - 0.5f);

        // Teleport: disable CC briefly to bypass collision
        _cc.enabled = false;
        Vector3 prevPos = transform.position;
        transform.position = dest;
        _cc.enabled = true;

        ConsumeCharge();
        OnPhaseStepPerformed?.Invoke(prevPos);
    }

    void ConsumeCharge()
    {
        if (_charges <= 0) return;
        _charges--;
        // Find first expired timer slot
        for (int i = 0; i < _chargeCount; i++)
        {
            if (_cooldownTimers[i] <= 0f)
            {
                _cooldownTimers[i] = _chargeCooldown;
                break;
            }
        }
    }

    void TickCooldowns()
    {
        for (int i = 0; i < _chargeCount; i++)
        {
            if (_cooldownTimers[i] > 0f)
            {
                _cooldownTimers[i] -= Time.deltaTime;
                if (_cooldownTimers[i] <= 0f)
                {
                    _cooldownTimers[i] = 0f;
                    if (_charges < _chargeCount)
                        _charges++;
                }
            }
        }
    }
}
