using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// HP sistemi — ZombieHealth1 ile aynı sözleşmeyi izler (IDamageable).
/// Ölümde GameEvents.FireEnemyKilled() → WaveManager wave takibi çalışır.
/// </summary>
public class ToySoldierHealth : MonoBehaviour, IDamageable
{
    [Header("Health")]
    public float maxHealth    = 80f;
    public float currentHealth;

    [Header("Audio")]
    public AudioClip deathSound;
    [Range(0f, 1f)] public float deathVolume = 1f;

    [Header("HP Bar")]
    public ZombieHealthBarUI hpBarPrefab;
    public Transform         hpBarTarget;

    [Header("Animation")]
    public Animator animator;
    public string   dieTrigger  = "Die";

    [Header("Death")]
    public float despawnDelay = 2.5f;
    public bool  disableCollidersOnDeath = true;

    private bool             isDead;
    private ZombieHealthBarUI hpBar;

    // ── Lifecycle ──────────────────────────────────────────────────────────

    void Awake()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();
        currentHealth = maxHealth;

        if (hpBarPrefab != null)
        {
            hpBar = Instantiate(hpBarPrefab);
            hpBar.target = (hpBarTarget != null) ? hpBarTarget : transform;
            hpBar.SetValue(currentHealth, maxHealth);
        }
    }

    // ── IDamageable ────────────────────────────────────────────────────────

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;

        if (hpBar != null)
        {
            hpBar.SetValue(currentHealth, maxHealth);
            hpBar.ShowAndAutoHide();
        }

        // Hit geri bildirimi (prefab'da varsa)
        GetComponent<ZombieHitFlash>()?.Flash();

        if (currentHealth <= 0f) Die();
    }

    // ── Death ──────────────────────────────────────────────────────────────

    void Die()
    {
        isDead = true;

        // Wave sayacını bilgilendir
        GameEvents.FireEnemyKilled(gameObject);

        if (deathSound != null)
            AudioSource.PlayClipAtPoint(deathSound, transform.position, deathVolume);

        // AI + shooter'ı durdur
        GetComponent<ToySoldierAI>()?.OnDeath();
        GetComponent<ToySoldierShooter>()?.SetFiring(false);

        // Collider'ları kapat
        if (disableCollidersOnDeath)
        {
            foreach (var col in GetComponentsInChildren<Collider>())
                col.enabled = false;
        }

        // Ölüm animasyonu
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            animator.ResetTrigger(dieTrigger);
            animator.SetTrigger(dieTrigger);
        }

        Destroy(gameObject, despawnDelay);
    }
}
