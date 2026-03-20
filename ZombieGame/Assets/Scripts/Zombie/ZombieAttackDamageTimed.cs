using System.Collections;
using UnityEngine;

public class ZombieAttackDamageTimed : MonoBehaviour
{
    [Header("Timing")]
    public float hitDelay = 0.6f;
    public float attackCooldown = 1.0f;

    [Header("Audio (YENİ - Buraya Taşındı)")]
    public AudioSource audioSource; // Inspector'dan ata (Zombi üzerindeki)
    public AudioClip attackSound;   // Saldırı sesini buraya koy

    [Header("Hit Check")]
    public float hitRadius = 1.2f;
    public float hitDistanceForward = 1.1f;
    public LayerMask playerLayer;

    [Header("Hit Origin (Recommended)")]
    public Transform hitOrigin;
    public float hitHeightOffset = 1.0f;

    [Header("Damage")]
    public float damage = 10f;

    [Header("Debug")]
    public bool debugLogs = false;

    private bool _canAttack = true;
    private Coroutine _co;

    void Awake()
    {
        // Eğer AudioSource atanmadıysa otomatik bulmaya çalış
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

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

        // --- BEKLEME (Animasyonun vuruş anına gelmesi) ---
        yield return new WaitForSeconds(hitDelay);

        // --- TAM VURDUĞU AN SES ÇAL ---
        if (audioSource != null && attackSound != null)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f); // Biraz varyasyon
            audioSource.PlayOneShot(attackSound);
        }

        // Hasarı ver
        DealDamage();

        // Geri kalan cooldown süresini bekle
        yield return new WaitForSeconds(attackCooldown);
        _canAttack = true;
    }

    private Vector3 GetHitCenter()
    {
        if (hitOrigin != null) return hitOrigin.position;
        return transform.position + Vector3.up * hitHeightOffset + transform.forward * hitDistanceForward;
    }

    private void DealDamage()
    {
        Vector3 center = GetHitCenter();
        Collider[] hits = Physics.OverlapSphere(center, hitRadius, playerLayer, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < hits.Length; i++)
        {
            var receiver = hits[i].GetComponent<PlayerDamageReceiver>();
            if (receiver != null)
            {
                receiver.TakeDamage(damage);
                DamageVignetteUI.Instance?.Play();

                var pc = hits[i].GetComponent<PlayerController2>();
                if (pc != null) pc.AddKnockbackFrom(transform.position);

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