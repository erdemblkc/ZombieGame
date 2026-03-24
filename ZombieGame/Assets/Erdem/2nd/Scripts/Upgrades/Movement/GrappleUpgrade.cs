using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Grapple — fire a hook toward the crosshair target (Q key).
/// Pulls the player to the hit point. If the target is an enemy,
/// the enemy is also pulled toward the player.
/// </summary>
public class GrappleUpgrade : MovementUpgradeBase
{
    [Header("Grapple Parameters")]
    [SerializeField] private float    _grappleRange  = 25f;
    [SerializeField] private float    _pullSpeed     = 20f;
    [SerializeField] private float    _cooldown      = 6f;
    [SerializeField] private float    _stopDistance  = 1.5f;
    [SerializeField] private KeyCode  _grappleKey    = KeyCode.Q;

    private bool      _isGrappling;
    private float     _cooldownTimer;
    private Vector3   _grappleTarget;
    private GameObject _grappleEnemy;    // non-null if hooked onto an enemy

    public bool  IsGrappling     => _isGrappling;
    public float CooldownRemaining => _cooldownTimer;

    public override void OnUpgradeDisabled()
    {
        _isGrappling = false;
        base.OnUpgradeDisabled();
    }

    void Update()
    {
        if (!IsRunnable) return;

        if (_cooldownTimer > 0f)
            _cooldownTimer -= Time.deltaTime;

        if (_isGrappling)
        {
            GrappleTick();
            return;
        }

        if (Input.GetKeyDown(_grappleKey) && _cooldownTimer <= 0f)
            TryFireGrapple();
    }

    void TryFireGrapple()
    {
        if (_cam == null) return;

        Ray ray = new Ray(_cam.transform.position, _cam.transform.forward);
        if (!Physics.Raycast(ray, out RaycastHit hit, _grappleRange)) return;

        _grappleTarget = hit.point;
        _grappleEnemy  = hit.collider.GetComponent<IDamageable>() != null ? hit.collider.gameObject : null;
        _isGrappling   = true;
    }

    void GrappleTick()
    {
        // If grappled onto an enemy, update target to its current position
        if (_grappleEnemy != null)
        {
            if (_grappleEnemy.activeInHierarchy)
            {
                _grappleTarget = _grappleEnemy.transform.position;

                // Also pull enemy toward player via NavMeshAgent
                var agent = _grappleEnemy.GetComponent<NavMeshAgent>();
                if (agent != null && agent.enabled)
                    agent.SetDestination(transform.position);
            }
            else
            {
                // Enemy died, end grapple
                EndGrapple();
                return;
            }
        }

        Vector3 dir = (_grappleTarget - transform.position);
        float dist  = dir.magnitude;

        if (dist < _stopDistance)
        {
            EndGrapple();
            return;
        }

        _cc.Move(dir.normalized * _pullSpeed * Time.deltaTime);
    }

    void EndGrapple()
    {
        _isGrappling   = false;
        _grappleEnemy  = null;
        _cooldownTimer = _cooldown;
    }
}
