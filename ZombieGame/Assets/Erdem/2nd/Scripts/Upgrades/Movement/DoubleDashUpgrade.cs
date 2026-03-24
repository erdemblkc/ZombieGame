using UnityEngine;

/// <summary>
/// Double Dash — replaces the built-in single dash with 2 independent charges,
/// each with its own cooldown. Dash direction follows movement input or forward.
/// Gravity is suppressed during the dash for a snappy feel.
/// </summary>
public class DoubleDashUpgrade : MovementUpgradeBase
{
    [Header("Double Dash Parameters")]
    [SerializeField] private float _dashForce     = 18f;
    [SerializeField] private float _dashDuration  = 0.15f;
    [SerializeField] private int   _chargeCount   = 2;
    [SerializeField] private float _chargeCooldown = 2.5f;

    private int     _charges;
    private float[] _cooldownTimers;
    private bool    _isDashing;
    private float   _dashTimer;
    private Vector3 _dashDir;

    public int   Charges     => _charges;
    public int   MaxCharges  => _chargeCount;

    public override void OnUpgradeEnabled(GameObject player)
    {
        base.OnUpgradeEnabled(player);
        _charges = _chargeCount;
        _cooldownTimers = new float[_chargeCount];
        _mods.SuppressDash = true;   // disable PlayerController2's built-in E-dash
    }

    public override void OnUpgradeDisabled()
    {
        if (_mods != null)
        {
            _mods.SuppressDash           = false;
            _mods.SuppressHorizontalMove = false;
            _mods.SuppressGravity        = false;
        }
        base.OnUpgradeDisabled();
    }

    void Update()
    {
        if (!IsRunnable) return;

        TickCooldowns();

        if (_isDashing)
        {
            _dashTimer -= Time.deltaTime;
            _mods.SuppressHorizontalMove = true;
            _mods.SuppressGravity        = true;
            _player.ResetVerticalVelocity();
            _cc.Move(_dashDir * _dashForce * Time.deltaTime);

            if (_dashTimer <= 0f)
                EndDash();

            return;
        }

        if (Input.GetKeyDown(KeyCode.E) && _charges > 0)
            StartDash();
    }

    void StartDash()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        Vector3 input = new Vector3(x, 0f, z);
        Vector3 moveDir = (_cc.transform.right * input.x + _cc.transform.forward * input.z);
        _dashDir = moveDir.sqrMagnitude > 0.01f ? moveDir.normalized : _cc.transform.forward;

        _isDashing  = true;
        _dashTimer  = _dashDuration;
        _charges--;
        // Start cooldown for the consumed charge slot
        _cooldownTimers[_charges] = _chargeCooldown;
    }

    void EndDash()
    {
        _isDashing                   = false;
        _mods.SuppressHorizontalMove = false;
        _mods.SuppressGravity        = false;
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

    // ── Evolution hook: ShoulderBash reads this ────────────────────────────
    public bool IsDashing => _isDashing;
    public Vector3 DashDirection => _dashDir;
}
