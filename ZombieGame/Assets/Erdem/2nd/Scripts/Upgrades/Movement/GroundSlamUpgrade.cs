using UnityEngine;

/// <summary>
/// Ground Slam — press C while airborne to plummet downward.
/// On landing, deals AoE damage and knockback to nearby enemies.
/// Horizontal movement is locked during the slam.
/// </summary>
public class GroundSlamUpgrade : MovementUpgradeBase
{
    [Header("Ground Slam Parameters")]
    [SerializeField] private float    _slamSpeed      = 30f;
    [SerializeField] private float    _slamRadius     = 4f;
    [SerializeField] private float    _slamDamage     = 40f;
    [SerializeField] private float    _knockbackForce = 8f;
    [SerializeField] private KeyCode  _slamKey        = KeyCode.C;
    [SerializeField] private LayerMask _enemyLayer    = ~0;

    private bool _isSlamming;

    public bool IsSlamming => _isSlamming;

    // ── Evolution hook: PredatorDrop reads this ────────────────────────────
    public event System.Action OnSlamLanded;

    public override void OnUpgradeDisabled()
    {
        if (_isSlamming) EndSlam(false);
        if (_mods != null)
        {
            _mods.SuppressHorizontalMove = false;
            _mods.SuppressGravity        = false;
        }
        base.OnUpgradeDisabled();
    }

    void Update()
    {
        if (!IsRunnable) return;

        if (_isSlamming)
        {
            _mods.SuppressHorizontalMove = true;
            _mods.SuppressGravity        = true;
            _cc.Move(Vector3.down * _slamSpeed * Time.deltaTime);

            if (_cc.isGrounded)
                EndSlam(true);

            return;
        }

        // Trigger: in air + slam key
        if (!_cc.isGrounded && Input.GetKeyDown(_slamKey))
            StartSlam();
    }

    /// <summary>Programmatically triggers a slam (used by PredatorDropEvolution).</summary>
    public void TriggerSlam()
    {
        if (!_isSlamming && !_cc.isGrounded)
            StartSlam();
    }

    void StartSlam()
    {
        _isSlamming = true;
        _mods.SuppressHorizontalMove = true;
        _mods.SuppressGravity        = true;
        _player.ResetVerticalVelocity();
    }

    void EndSlam(bool landed)
    {
        _isSlamming                  = false;
        _mods.SuppressHorizontalMove = false;
        _mods.SuppressGravity        = false;

        if (landed) PerformAoE();
    }

    void PerformAoE()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, _slamRadius, _enemyLayer);
        foreach (var col in hits)
        {
            if (col.gameObject == gameObject) continue;

            // Damage
            var damageable = col.GetComponent<IDamageable>();
            damageable?.TakeDamage(_slamDamage);

            // Knockback via rigidbody if present
            var rb = col.GetComponent<Rigidbody>();
            if (rb != null)
                rb.AddExplosionForce(_knockbackForce * 100f, transform.position, _slamRadius, 1f, ForceMode.Force);
        }

        OnSlamLanded?.Invoke();
    }
}
