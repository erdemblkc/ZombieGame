using System.Collections;
using UnityEngine;

public class ZombieAttackDamageTimed : MonoBehaviour
{
    [Header("Timing")]
    public float hitDelay = 0.14f;
    public float attackCooldown = 1.0f;

    [Header("Hit Check")]
    public float hitRadius = 1.2f;
    public float hitDistanceForward = 1.1f;
    public LayerMask playerLayer;

    [Header("Damage")]
    public float damage = 10f;

    [Header("Knockback")]
    public bool applyKnockback = true;

    [Header("Debug")]
    public bool debugLogs = true;

    private bool _canAttack = true;
    private Coroutine _co;

    public void DoAttack()
    {
        if (!_canAttack) return;

        if (_co != null) StopCoroutine(_co);
        _co = StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        _canAttack = false;

        if (debugLogs) Debug.Log($"[{name}] Attack started. Will hit after {hitDelay:0.00}s");
        yield return new WaitForSeconds(hitDelay);

        DealDamageAndKnockback();

        yield return new WaitForSeconds(attackCooldown);
        _canAttack = true;
    }

    private void DealDamageAndKnockback()
    {
        Vector3 center = transform.position + transform.forward * hitDistanceForward;
        Collider[] hits = Physics.OverlapSphere(center, hitRadius, playerLayer, QueryTriggerInteraction.Ignore);

        if (debugLogs) Debug.Log($"[{name}] Hit check: found {hits.Length} colliders.");

        for (int i = 0; i < hits.Length; i++)
        {
            // Damage
            var receiver = hits[i].GetComponent<PlayerDamageReceiver>();
            if (receiver != null)
            {
                receiver.TakeDamage(damage);

                // ✅ Knockback'i PlayerController2 üstünden uygula
                if (applyKnockback)
                {
                    var pc = hits[i].GetComponent<PlayerController2>();
                    if (pc != null)
                    {
                        pc.AddKnockbackFrom(transform.position);
                        if (debugLogs) Debug.Log($"[{name}] Knockback applied via PlayerController2.");
                    }
                    else
                    {
                        if (debugLogs) Debug.LogWarning($"[{name}] PlayerController2 not found on player collider object: {hits[i].name}");
                    }
                }

                if (debugLogs) Debug.Log($"[{name}] Hit player: {hits[i].name}");
                return;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 center = transform.position + transform.forward * hitDistanceForward;
        Gizmos.DrawWireSphere(center, hitRadius);
    }
}
