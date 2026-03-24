using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class ZombieHealth1 : MonoBehaviour, IDamageable
{
    [Header("Health")]
    public float maxHealth = 100f;
    public float currentHealth = 100f;

    [Header("Audio - Death (YENİ)")]
    public AudioClip deathVoiceSound; // Zombinin son nefesi/böğürmesi
    [Range(0f, 1f)] public float voiceVolume = 1.0f;

    public AudioClip bodyFallSound;   // Lego parçalarının dağılma sesi
    [Range(0f, 1f)] public float fallVolume = 1.0f;

    [Header("HP Bar")]
    public ZombieHealthBarUI hpBarPrefab;
    public Transform hpBarTarget;

    private ZombieHealthBarUI hpBar;

    [Header("Animation")]
    public Animator animator;
    public string dieTrigger = "Die";

    [Header("Death Timing")]
    [Tooltip("Die anim bitince yerde kaç saniye kalsın?")]
    public float stayOnGroundTime = 0.6f;

    [Tooltip("Die anim klibinin süresi. 0 bırakılırsa despawnDelay kullanılır.")]
    public float dieAnimDuration = 0f;

    [Tooltip("dieAnimDuration=0 ise toplam bekleme (anim + yerde kalma) yerine direkt bu kullanılır.")]
    public float despawnDelay = 1.2f;

    [Header("Death Options")]
    public bool disableCollidersOnDeath = true;

    private bool isDead;

    void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        currentHealth = maxHealth;
        if (hpBarPrefab != null)
        {
            hpBar = Instantiate(hpBarPrefab);
            hpBar.target = (hpBarTarget != null) ? hpBarTarget : transform;
            hpBar.SetValue(currentHealth, maxHealth);
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        if (hpBar != null)
        {
            hpBar.SetValue(currentHealth, maxHealth);
            hpBar.ShowAndAutoHide();
        }

        // Hit feedback
        GetComponent<ZombieHitFlash>()?.Flash();
        GetComponent<ZombieHitReact>()?.React(Camera.main != null ? Camera.main.transform.position : transform.position);

        if (currentHealth <= 0f)
            Die();
    }

    void Die()
    {
        isDead = true;

        // Notify upgrade system (MomentumSurge, etc.)
        GameEvents.FireEnemyKilled(gameObject);

        // --- ÇİFT SES SİSTEMİ ---

        // 1. Zombi Sesi (Böğürme)
        if (deathVoiceSound != null)
        {
            // PlayClipAtPoint, obje yok olsa bile sesi o noktada (3D) çalar.
            AudioSource.PlayClipAtPoint(deathVoiceSound, transform.position, voiceVolume);
        }

        // 2. Dağılma Sesi (Plastik parçalar)
        if (bodyFallSound != null)
        {
            AudioSource.PlayClipAtPoint(bodyFallSound, transform.position, fallVolume);
        }

        // AI/Agent kapat
        var ai = GetComponent<ZombieAI_Follow>();
        if (ai != null) ai.enabled = false;

        var agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.enabled = false;
        }

        // Colliders kapat
        if (disableCollidersOnDeath)
        {
            foreach (var col in GetComponentsInChildren<Collider>())
                col.enabled = false;
        }

        // Die anim
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            animator.ResetTrigger(dieTrigger);
            animator.SetTrigger(dieTrigger);
        }

        StartCoroutine(DespawnRoutine());
    }

    IEnumerator DespawnRoutine()
    {
        float wait;

        if (dieAnimDuration > 0f)
            wait = dieAnimDuration + stayOnGroundTime;
        else
            wait = despawnDelay;

        yield return new WaitForSeconds(wait);
        Destroy(gameObject);
    }
}