using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Teddy Boss — 3 fazlı boss AI.
/// Animatör olmadan da çalışır: faz geçişleri renk + scale ile görünür.
/// Model/animasyon gelince sadece Inspector'daki trigger isimlerini doldur.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class TeddyBoss : MonoBehaviour, IDamageable
{
    // ──────────────────────────────────────────────
    // ENUM
    // ──────────────────────────────────────────────
    public enum BossPhase { Phase1, Phase2, Phase3, Dead }

    // ──────────────────────────────────────────────
    // HEALTH
    // ──────────────────────────────────────────────
    [Header("Health")]
    public float maxHealth = 600f;

    [Tooltip("Faz 2 başlangıç eşiği (% olarak)")]
    [Range(0f, 1f)] public float phase2Threshold = 0.60f;

    [Tooltip("Faz 3 başlangıç eşiği (% olarak)")]
    [Range(0f, 1f)] public float phase3Threshold = 0.25f;

    // ──────────────────────────────────────────────
    // FAZ 1 — Yavaş, Ağır
    // ──────────────────────────────────────────────
    [Header("Faz 1 — Yavaş & Ağır")]
    public float phase1Speed       = 3.5f;
    public float phase1Damage      = 25f;
    public float phase1AttackCooldown = 2.5f;
    public float phase1AttackRange = 2.2f;

    // ──────────────────────────────────────────────
    // FAZ 2 — Çılgın Koşu + Charge
    // ──────────────────────────────────────────────
    [Header("Faz 2 — Hızlı & Charge")]
    public float phase2Speed       = 6f;
    public float phase2Damage      = 20f;
    public float phase2AttackCooldown = 1.5f;
    public float phase2AttackRange = 2.2f;

    [Tooltip("Charge hızı — kısa süre için bu hıza fırlar")]
    public float chargeSpeed       = 18f;
    [Tooltip("Charge mesafesi — oyuncuya bu mesafeden yakın olunca charge başlar")]
    public float chargeRange       = 12f;
    [Tooltip("Charge arası minimum bekleme (saniye)")]
    public float chargeInterval    = 5f;
    [Tooltip("Charge süresi (saniye)")]
    public float chargeDuration    = 0.6f;

    // ──────────────────────────────────────────────
    // FAZ 3 — Sürünme, Ani & Tehlikeli
    // ──────────────────────────────────────────────
    [Header("Faz 3 — Sürünme & Ani Saldırı")]
    public float phase3Speed       = 4.5f;
    public float phase3Damage      = 35f;
    public float phase3AttackCooldown = 0.8f;
    public float phase3AttackRange = 2.5f;

    [Tooltip("Faz 3'te Y ekseninde scale küçülmesi (sürünme simülasyonu)")]
    public float phase3ScaleY      = 0.55f;

    [Tooltip("Burst hareketi arası bekleme — faz 3'te rastgele pozisyona fırlar")]
    public float burstInterval     = 3f;
    public float burstSpeed        = 14f;
    public float burstDuration     = 0.35f;

    // ──────────────────────────────────────────────
    // PLACEHOLDER RENKLER (model yokken fazları gösterir)
    // ──────────────────────────────────────────────
    [Header("Placeholder Renk (Model Yokken)")]
    public Color phase1Color = new Color(0.8f, 0.6f, 0.4f);  // kahve
    public Color phase2Color = new Color(1f,   0.25f, 0.1f); // kırmızı-turuncu
    public Color phase3Color = new Color(0.4f, 0f,    0f);   // koyu kırmızı

    // ──────────────────────────────────────────────
    // ANİMATÖR (OPSİYONEL — model gelince doldur)
    // ──────────────────────────────────────────────
    [Header("Animatör (Opsiyonel — Model Gelince Doldur)")]
    public Animator animator;
    public string anim_Walk       = "IsWalking";
    public string anim_Attack     = "Attack";
    public string anim_Charge     = "Charge";
    public string anim_Phase2     = "Phase2";
    public string anim_Phase3     = "Phase3";
    public string anim_Die        = "Die";

    // ──────────────────────────────────────────────
    // SES (OPSİYONEL)
    // ──────────────────────────────────────────────
    [Header("Ses (Opsiyonel)")]
    public AudioClip phase2RoarClip;
    public AudioClip phase3RoarClip;
    public AudioClip chargeClip;
    public AudioClip deathClip;
    [Tooltip("Faz 3 ölümü için müzik tetikleyici — GameEvents ile override edilebilir")]
    public AudioClip sadMelodyClip;

    // ──────────────────────────────────────────────
    // INTERNAL
    // ──────────────────────────────────────────────
    BossPhase currentPhase = BossPhase.Phase1;
    float currentHealth;
    Transform player;
    NavMeshAgent agent;
    AudioSource audioSource;
    Renderer[] renderers;
    Vector3 originalScale;

    float nextAttackTime;
    float nextChargeTime;
    float nextBurstTime;
    bool isCharging;
    bool isBursting;
    bool isDead;

    // ──────────────────────────────────────────────
    // INIT
    // ──────────────────────────────────────────────
    void Awake()
    {
        agent        = GetComponent<NavMeshAgent>();
        audioSource  = GetComponent<AudioSource>();
        renderers    = GetComponentsInChildren<Renderer>();
        originalScale = transform.localScale;

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        currentHealth = maxHealth;
    }

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        ApplyPhaseSettings();
        SetRendererColor(phase1Color);

        nextChargeTime = Time.time + chargeInterval;
        nextBurstTime  = Time.time + burstInterval;
    }

    // ──────────────────────────────────────────────
    // DAMAGE
    // ──────────────────────────────────────────────
    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        currentHealth  = Mathf.Max(0f, currentHealth);

        CheckPhaseTransition();

        if (currentHealth <= 0f)
            Die();
    }

    // ──────────────────────────────────────────────
    // FAZ GEÇİŞİ
    // ──────────────────────────────────────────────
    void CheckPhaseTransition()
    {
        float ratio = currentHealth / maxHealth;

        if (currentPhase == BossPhase.Phase1 && ratio <= phase2Threshold)
            EnterPhase2();
        else if (currentPhase == BossPhase.Phase2 && ratio <= phase3Threshold)
            EnterPhase3();
    }

    void EnterPhase2()
    {
        currentPhase = BossPhase.Phase2;
        ApplyPhaseSettings();
        SetRendererColor(phase2Color);
        PlayClip(phase2RoarClip);
        TriggerAnim(anim_Phase2);
        nextChargeTime = Time.time + 1f; // kısa süre içinde ilk charge
        Debug.Log("[TeddyBoss] FAZ 2 BAŞLADI — Charge saldırıları aktif");
    }

    void EnterPhase3()
    {
        currentPhase = BossPhase.Phase3;
        ApplyPhaseSettings();
        SetRendererColor(phase3Color);
        PlayClip(phase3RoarClip);
        TriggerAnim(anim_Phase3);

        // Sürünme simülasyonu — Y scale küçült
        transform.localScale = new Vector3(originalScale.x, originalScale.y * phase3ScaleY, originalScale.z);
        nextBurstTime = Time.time + 0.5f;
        Debug.Log("[TeddyBoss] FAZ 3 BAŞLADI — Sürünme modu, ani saldırılar");
    }

    void ApplyPhaseSettings()
    {
        switch (currentPhase)
        {
            case BossPhase.Phase1:
                agent.speed = phase1Speed;
                break;
            case BossPhase.Phase2:
                agent.speed = phase2Speed;
                break;
            case BossPhase.Phase3:
                agent.speed = phase3Speed;
                break;
        }
    }

    float CurrentDamage => currentPhase switch
    {
        BossPhase.Phase1 => phase1Damage,
        BossPhase.Phase2 => phase2Damage,
        BossPhase.Phase3 => phase3Damage,
        _ => 0f
    };

    float CurrentAttackRange => currentPhase switch
    {
        BossPhase.Phase1 => phase1AttackRange,
        BossPhase.Phase2 => phase2AttackRange,
        BossPhase.Phase3 => phase3AttackRange,
        _ => 0f
    };

    float CurrentAttackCooldown => currentPhase switch
    {
        BossPhase.Phase1 => phase1AttackCooldown,
        BossPhase.Phase2 => phase2AttackCooldown,
        BossPhase.Phase3 => phase3AttackCooldown,
        _ => 99f
    };

    // ──────────────────────────────────────────────
    // UPDATE
    // ──────────────────────────────────────────────
    void Update()
    {
        if (isDead || player == null) return;
        if (!agent.isOnNavMesh) return;
        if (isCharging || isBursting) return;

        float dist = Vector3.Distance(
            new Vector3(transform.position.x, 0, transform.position.z),
            new Vector3(player.position.x,    0, player.position.z));

        // Faz 2 özel: charge zamanı geldi mi?
        if (currentPhase == BossPhase.Phase2 && Time.time >= nextChargeTime && dist <= chargeRange)
        {
            StartCoroutine(ChargeRoutine());
            return;
        }

        // Faz 3 özel: burst hareketi
        if (currentPhase == BossPhase.Phase3 && Time.time >= nextBurstTime)
        {
            StartCoroutine(BurstRoutine());
            return;
        }

        // Normal yaklaşma
        if (dist > CurrentAttackRange)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
            SetAnim(anim_Walk, true);
        }
        else
        {
            // Saldırı mesafesindeyiz
            agent.isStopped = true;
            agent.ResetPath();
            SetAnim(anim_Walk, false);

            if (Time.time >= nextAttackTime)
            {
                DoAttack();
                nextAttackTime = Time.time + CurrentAttackCooldown;
            }
        }
    }

    // ──────────────────────────────────────────────
    // SALDIRI
    // ──────────────────────────────────────────────
    void DoAttack()
    {
        TriggerAnim(anim_Attack);

        // Oyuncu saldırı menzilinde mi kontrol et
        float dist = Vector3.Distance(transform.position, player.position);
        if (dist <= CurrentAttackRange + 0.5f)
        {
            player.GetComponent<IDamageable>()?.TakeDamage(CurrentDamage);
        }
    }

    // ──────────────────────────────────────────────
    // FAZ 2 — CHARGE
    // ──────────────────────────────────────────────
    IEnumerator ChargeRoutine()
    {
        isCharging = true;
        TriggerAnim(anim_Charge);
        PlayClip(chargeClip);
        Debug.Log("[TeddyBoss] CHARGE!");

        Vector3 chargeTarget = player.position;
        agent.speed = chargeSpeed;
        agent.isStopped = false;
        agent.SetDestination(chargeTarget);

        float elapsed = 0f;
        while (elapsed < chargeDuration)
        {
            elapsed += Time.deltaTime;

            // Charge esnasında oyuncuya çarptı mı?
            float dist = Vector3.Distance(transform.position, player.position);
            if (dist < 1.5f)
            {
                player.GetComponent<IDamageable>()?.TakeDamage(phase2Damage * 1.5f);
                break;
            }
            yield return null;
        }

        agent.speed = phase2Speed;
        isCharging = false;
        nextChargeTime = Time.time + chargeInterval;
    }

    // ──────────────────────────────────────────────
    // FAZ 3 — BURST HAREKETİ
    // ──────────────────────────────────────────────
    IEnumerator BurstRoutine()
    {
        isBursting = true;

        // Oyuncunun etrafına rastgele bir nokta seç
        Vector3 randomOffset = Random.insideUnitSphere * 3f;
        randomOffset.y = 0f;
        Vector3 burstTarget = player.position + randomOffset;

        agent.speed = burstSpeed;
        agent.isStopped = false;
        agent.SetDestination(burstTarget);

        float elapsed = 0f;
        while (elapsed < burstDuration)
        {
            elapsed += Time.deltaTime;

            float dist = Vector3.Distance(transform.position, player.position);
            if (dist < phase3AttackRange)
            {
                player.GetComponent<IDamageable>()?.TakeDamage(phase3Damage);
                break;
            }
            yield return null;
        }

        agent.speed = phase3Speed;
        isBursting = false;
        nextBurstTime = Time.time + burstInterval;
    }

    // ──────────────────────────────────────────────
    // ÖLÜM
    // ──────────────────────────────────────────────
    void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("[TeddyBoss] ÖLDÜ — Faz 3 ölüm anı");

        agent.isStopped = true;
        agent.ResetPath();
        agent.enabled = false;

        foreach (var col in GetComponentsInChildren<Collider>())
            col.enabled = false;

        TriggerAnim(anim_Die);
        GameEvents.FireEnemyKilled(gameObject);

        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        // Sessizlik — 2 saniye
        yield return new WaitForSeconds(2f);

        // Üzücü melodi
        if (sadMelodyClip != null)
            AudioSource.PlayClipAtPoint(sadMelodyClip, transform.position, 0.8f);
        else if (deathClip != null)
            AudioSource.PlayClipAtPoint(deathClip, transform.position, 0.8f);

        yield return new WaitForSeconds(1.5f);
        Destroy(gameObject);
    }

    // ──────────────────────────────────────────────
    // YARDIMCI METODLAR
    // ──────────────────────────────────────────────
    void SetRendererColor(Color color)
    {
        foreach (var r in renderers)
        {
            if (r == null) continue;
            foreach (var mat in r.materials)
                if (mat.HasProperty("_Color"))
                    mat.color = color;
        }
    }

    void TriggerAnim(string triggerName)
    {
        if (animator == null || animator.runtimeAnimatorController == null) return;
        if (string.IsNullOrEmpty(triggerName)) return;
        animator.SetTrigger(triggerName);
    }

    void SetAnim(string boolName, bool value)
    {
        if (animator == null || animator.runtimeAnimatorController == null) return;
        if (string.IsNullOrEmpty(boolName)) return;
        animator.SetBool(boolName, value);
    }

    void PlayClip(AudioClip clip)
    {
        if (clip == null || audioSource == null) return;
        audioSource.PlayOneShot(clip);
    }

    // ──────────────────────────────────────────────
    // DEBUG GIZMO — Editörde fazları görselleştir
    // ──────────────────────────────────────────────
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, phase1AttackRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chargeRange);
    }
}
