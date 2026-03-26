using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Kalay Asker zombie — önden hasar almaz (kalkan), arkadan normal hasar.
/// ZombieHealth1 yerine bu script IDamageable'ı implement eder.
/// Animatör opsiyonel — Inspector'dan doldurulur.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class KalayAskerZombie : MonoBehaviour, IDamageable
{
    [Header("Health")]
    public float maxHealth = 150f;

    [Header("Kalkan")]
    [Tooltip("Önden gelen hasar bu açı içindeyse bloklanır (derece, yarı açı)")]
    public float shieldAngle = 60f;
    [Tooltip("Blok animasyonu — model gelince doldur")]
    public string anim_Block = "Block";
    [Tooltip("Bloklanmış hasar oranı — 0 = sıfır hasar, 0.1 = %10 geçer")]
    [Range(0f, 1f)] public float blockedDamageRatio = 0f;

    [Header("Placeholder Renk")]
    public Color shieldActiveColor   = new Color(0.7f, 0.85f, 1f); // metal mavi
    public Color shieldDisabledColor = new Color(1f,   0.4f,  0.4f); // kırmızı (arkadan vurulunca)

    [Header("Animatör (Opsiyonel)")]
    public Animator animator;
    public string anim_Walk   = "IsWalking";
    public string anim_Attack = "Attack";
    public string anim_Die    = "Die";

    [Header("Ses (Opsiyonel)")]
    public AudioClip blockSound;
    public AudioClip deathSound;

    // ── Internal ──────────────────────────────────────────────────────────
    float currentHealth;
    bool  isDead;
    bool  lastHitWasBlocked;
    Renderer[] renderers;
    AudioSource audioSource;

    void Awake()
    {
        currentHealth = maxHealth;
        renderers     = GetComponentsInChildren<Renderer>();
        audioSource   = GetComponent<AudioSource>();
        if (animator == null) animator = GetComponentInChildren<Animator>();

        SetColor(shieldActiveColor);
    }

    void Start()
    {
        var agent = GetComponent<NavMeshAgent>();
        if (agent != null) agent.stoppingDistance = 0f;
    }

    // ── IDamageable ───────────────────────────────────────────────────────

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        // Hasarın nereden geldiğini bul — Main Camera = oyuncu bakış noktası
        Vector3 attackerPos = Camera.main != null ? Camera.main.transform.position : transform.position + transform.forward * 5f;
        bool blocked = IsAttackBlocked(attackerPos);

        if (blocked)
        {
            // Kalkan aktif
            float actualDamage = amount * blockedDamageRatio;
            currentHealth -= actualDamage;
            PlayClip(blockSound);
            TriggerAnim(anim_Block);
            FlashColor(shieldActiveColor);
            Debug.Log($"[KalayAsker] BLOK! Hasar: {amount} → {actualDamage} geçti.");
        }
        else
        {
            // Arka taraf — tam hasar
            currentHealth -= amount;
            GetComponent<ZombieHitFlash>()?.Flash();
            GetComponent<ZombieHitReact>()?.React(attackerPos);
            FlashColor(shieldDisabledColor);
            Debug.Log($"[KalayAsker] Arka vuruş — {amount} hasar aldı.");
        }

        currentHealth = Mathf.Max(0f, currentHealth);
        if (currentHealth <= 0f) Die();
    }

    // ── Yön Kontrolü ─────────────────────────────────────────────────────

    /// <summary>
    /// Saldırı önden mi geliyor?
    /// Zombinin forward yönüyle saldırı yönü arasındaki açıya bakılır.
    /// </summary>
    bool IsAttackBlocked(Vector3 attackerPos)
    {
        Vector3 toAttacker = (attackerPos - transform.position);
        toAttacker.y = 0f;
        toAttacker.Normalize();

        float dot = Vector3.Dot(transform.forward, toAttacker);
        float angle = Mathf.Acos(Mathf.Clamp(dot, -1f, 1f)) * Mathf.Rad2Deg;

        // Açı shieldAngle'dan küçükse önden geliyor → blok
        return angle < shieldAngle;
    }

    // ── Death ─────────────────────────────────────────────────────────────

    void Die()
    {
        if (isDead) return;
        isDead = true;

        GameEvents.FireEnemyKilled(gameObject);
        PlayClip(deathSound);
        TriggerAnim(anim_Die);

        var agent = GetComponent<NavMeshAgent>();
        if (agent != null) { agent.isStopped = true; agent.enabled = false; }

        var ai = GetComponent<ZombieAI_Follow>();
        if (ai != null) ai.enabled = false;

        foreach (var col in GetComponentsInChildren<Collider>())
            col.enabled = false;

        Destroy(gameObject, 1.5f);
    }

    // ── Yardımcılar ───────────────────────────────────────────────────────

    void SetColor(Color c)
    {
        foreach (var r in renderers)
            if (r != null)
                foreach (var mat in r.materials)
                    if (mat.HasProperty("_Color")) mat.color = c;
    }

    void FlashColor(Color c)
    {
        SetColor(c);
        // Kısa flash — 0.15s sonra normal renge dön
        CancelInvoke(nameof(ResetColor));
        Invoke(nameof(ResetColor), 0.15f);
    }

    void ResetColor() => SetColor(shieldActiveColor);

    void TriggerAnim(string trigger)
    {
        if (animator == null || animator.runtimeAnimatorController == null) return;
        if (string.IsNullOrEmpty(trigger)) return;
        animator.SetTrigger(trigger);
    }

    void PlayClip(AudioClip clip)
    {
        if (clip == null || audioSource == null) return;
        audioSource.PlayOneShot(clip);
    }

    // ── Gizmo ─────────────────────────────────────────────────────────────

    void OnDrawGizmosSelected()
    {
        // Kalkan açısını editörde göster
        Gizmos.color = Color.cyan;
        Vector3 left  = Quaternion.Euler(0,  shieldAngle, 0) * transform.forward;
        Vector3 right = Quaternion.Euler(0, -shieldAngle, 0) * transform.forward;
        Gizmos.DrawRay(transform.position, left  * 2f);
        Gizmos.DrawRay(transform.position, right * 2f);
        Gizmos.DrawRay(transform.position, transform.forward * 2f);
    }
}
