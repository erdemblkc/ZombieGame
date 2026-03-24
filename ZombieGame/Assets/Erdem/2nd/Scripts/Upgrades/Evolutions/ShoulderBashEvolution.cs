using UnityEngine;

/// <summary>
/// Shoulder Bash Evolution — requires Jetpack + DoubleDash.
/// While the DoubleDash is active in air, enemies in the dash path
/// receive heavy damage and knockback (shoulder collision).
/// </summary>
public class ShoulderBashEvolution : MonoBehaviour, IEvolution
{
    [SerializeField] private float _bashDamage    = 35f;
    [SerializeField] private float _bashRadius    = 1.2f;
    [SerializeField] private float _bashKnockback = 12f;

    private DoubleDashUpgrade _dash;
    private bool              _wasAirDashing;

    public void OnEvolutionEnabled(GameObject player)
    {
        _dash = player.GetComponent<DoubleDashUpgrade>();
        _wasAirDashing = false;
        Debug.Log("[ShoulderBash] Evolution active.");
    }

    public void OnEvolutionDisabled()
    {
        _dash = null;
    }

    void Update()
    {
        if (_dash == null) return;

        var cc       = GetComponent<CharacterController>();
        bool inAir   = cc != null && !cc.isGrounded;
        bool dashing = _dash.IsDashing;

        // Detect the moment dash starts while airborne
        if (dashing && inAir && !_wasAirDashing)
            PerformBash(_dash.DashDirection);

        _wasAirDashing = dashing && inAir;
    }

    void PerformBash(Vector3 direction)
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position + direction * _bashRadius,
            _bashRadius);

        foreach (var col in hits)
        {
            if (col.gameObject == gameObject) continue;

            col.GetComponent<IDamageable>()?.TakeDamage(_bashDamage);

            var rb = col.GetComponent<Rigidbody>();
            if (rb != null)
                rb.AddForce(direction * _bashKnockback, ForceMode.Impulse);
        }
    }
}
