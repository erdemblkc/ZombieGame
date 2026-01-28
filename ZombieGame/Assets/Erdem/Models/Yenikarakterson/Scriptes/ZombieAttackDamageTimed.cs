using System.Collections;
using UnityEngine;

public class ZombieAttackDamageTimed : MonoBehaviour
{
    [Header("Timing")]
    public float hitDelay = 0.6f;
    public float attackCooldown = 1.0f;

    [Header("Hit Check")]
    public float hitRadius = 1.2f;

    [Tooltip("HitOrigin yoksa fallback olarak transform.forward ile öne kaydırır.")]
    public float hitDistanceForward = 1.1f;

    public LayerMask playerLayer;

    [Header("Hit Origin (Recommended)")]
    [Tooltip("Boş bırakılırsa, transform.position + height/forward ile hesaplar.")]
    public Transform hitOrigin;

    [Tooltip("HitOrigin boşsa kullanılacak Y offset.")]
    public float hitHeightOffset = 1.0f;

    [Header("Damage")]
    public float damage = 10f;

    [Header("Debug")]
    public bool debugLogs = false;

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

        if (debugLogs)
            Debug.Log($"[{name}] Attack started. Will hit after {hitDelay:0.00}s");

        yield return new WaitForSeconds(hitDelay);

        DealDamage();

        yield return new WaitForSeconds(attackCooldown);
        _canAttack = true;
    }

    private Vector3 GetHitCenter()
    {
        // 1) Eğer HitOrigin atanmışsa: direkt onu kullan
        if (hitOrigin != null)
            return hitOrigin.position;

        // 2) Yoksa: pivot + yukarı offset + ileri offset
        return transform.position
             + Vector3.up * hitHeightOffset
             + transform.forward * hitDistanceForward;
    }

    private void DealDamage()
    {
        Vector3 center = GetHitCenter();

        Collider[] hits = Physics.OverlapSphere(center, hitRadius, playerLayer, QueryTriggerInteraction.Ignore);

        if (debugLogs)
            Debug.Log($"[{name}] Hit check: found {hits.Length} colliders.");

        for (int i = 0; i < hits.Length; i++)
        {
            var receiver = hits[i].GetComponent<PlayerDamageReceiver>();
            if (receiver != null)
            {
                receiver.TakeDamage(damage);
                DamageVignetteUI.Instance?.Play();


                var pc = hits[i].GetComponent<PlayerController2>();
                if (pc != null)
                    pc.AddKnockbackFrom(transform.position);

                if (debugLogs)
                    Debug.Log($"[{name}] Hit player: {hits[i].name}");

                return;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Vector3 center = GetHitCenter();
        Gizmos.DrawWireSphere(center, hitRadius);
    }
}
