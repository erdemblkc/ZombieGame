using UnityEngine;

/// <summary>
/// Jetpack — hold Jump while airborne to apply upward thrust.
/// Fuel tank refills when grounded.
/// </summary>
public class JetpackUpgrade : MovementUpgradeBase
{
    [Header("Jetpack Parameters")]
    [SerializeField] private float _fuelMax          = 3f;
    [SerializeField] private float _thrustForce      = 14f;
    [SerializeField] private float _fuelRechargeRate = 1.5f;

    private float _fuelCurrent;
    private bool  _wasGrounded;

    // ── Public read-only for HUD ──────────────────────────────────────────
    public float FuelCurrent => _fuelCurrent;
    public float FuelMax     => _fuelMax;

    public override void OnUpgradeEnabled(GameObject player)
    {
        base.OnUpgradeEnabled(player);
        _fuelCurrent = _fuelMax;
        _wasGrounded = _cc.isGrounded;
    }

    public override void OnUpgradeDisabled()
    {
        // Always restore gravity suppression
        if (_mods != null) _mods.SuppressGravity = false;
        base.OnUpgradeDisabled();
    }

    void Update()
    {
        if (!IsRunnable) return;

        bool grounded = _cc.isGrounded;

        if (grounded)
        {
            // Recharge fuel while on ground
            _fuelCurrent = Mathf.Min(_fuelMax, _fuelCurrent + _fuelRechargeRate * Time.deltaTime);
            _mods.SuppressGravity = false;
            _wasGrounded = true;
            return;
        }

        // In air: thrust while Space held and fuel available
        if (Input.GetKey(KeyCode.Space) && _fuelCurrent > 0f)
        {
            _fuelCurrent -= Time.deltaTime;
            _fuelCurrent  = Mathf.Max(0f, _fuelCurrent);

            // Take over gravity so PlayerController2 doesn't fight the thrust
            _mods.SuppressGravity = true;
            _player.ResetVerticalVelocity();

            _cc.Move(Vector3.up * _thrustForce * Time.deltaTime);
        }
        else
        {
            _mods.SuppressGravity = false;
        }

        _wasGrounded = grounded;
    }
}
