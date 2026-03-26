using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Kurmalı Zombie — Duruyor → aniden 3x hız sprint pattern.
/// Zembereği dolu: normal hızla yürür.
/// Yavaşlar → durur → şarj olur → aniden hız patlaması.
/// Animatör opsiyonel — durum renk + scale ile görünür.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class KurmalıZombie : MonoBehaviour, IDamageable
{
    [Header("Health")]
    public float maxHealth = 80f;

    [Header("Hareket")]
    public float normalSpeed   = 2f;
    [Tooltip("Sprint hız çarpanı (normalSpeed * bu)")]
    public float sprintMul     = 3f;

    [Header("Pattern Zamanlaması")]
    [Tooltip("Her sprint öncesi duraklama süresi (saniye)")]
    public float windupTime    = 1.4f;
    [Tooltip("Sprint süresi (saniye)")]
    public float sprintTime    = 0.9f;
    [Tooltip("Sprint sonrası toparlanma süresi")]
    public float cooldownTime  = 2f;
    [Tooltip("İlk sprint başlamadan önceki rastgele bekleme")]
    public float initialDelay  = 1f;

    [Header("Saldırı")]
    public float attackRange   = 1.8f;
    public float attackDamage  = 18f;
    public float attackCooldown = 1.5f;

    [Header("Placeholder Renk")]
    public Color normalColor  = new Color(0.5f, 0.8f, 0.5f);  // yeşil
    public Color windupColor  = new Color(1f,   0.85f, 0f);   // sarı (şarj oluyor)
    public Color sprintColor  = new Color(1f,   0.3f,  0f);   // turuncu (sprint)

    [Header("Animatör (Opsiyonel)")]
    public Animator animator;
    public string anim_Walk   = "IsWalking";
    public string anim_Sprint = "Sprint";
    public string anim_Windup = "Windup";
    public string anim_Attack = "Attack";
    public string anim_Die    = "Die";

    [Header("Ses (Opsiyonel)")]
    public AudioClip windupSound;
    public AudioClip sprintSound;
    public AudioClip deathSound;

    // ── State ─────────────────────────────────────────────────────────────

    enum State { Normal, Windup, Sprint, Cooldown }

    State     currentState = State.Normal;
    float     currentHealth;
    bool      isDead;
    Transform player;
    NavMeshAgent agent;
    Renderer[] renderers;
    AudioSource audioSource;
    float     nextAttackTime;

    // ── Lifecycle ─────────────────────────────────────────────────────────

    void Awake()
    {
        currentHealth = maxHealth;
        agent         = GetComponent<NavMeshAgent>();
        renderers     = GetComponentsInChildren<Renderer>();
        audioSource   = GetComponent<AudioSource>();
        if (animator == null) animator = GetComponentInChildren<Animator>();

        agent.speed = normalSpeed;
        SetColor(normalColor);
    }

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        StartCoroutine(PatternLoop());
    }

    // ── Pattern Loop ──────────────────────────────────────────────────────

    IEnumerator PatternLoop()
    {
        yield return new WaitForSeconds(Random.Range(0f, initialDelay));

        while (!isDead)
        {
            // COOLDOWN / NORMAL YÜRÜYÜŞ
            currentState = State.Normal;
            SetColor(normalColor);
            SetAnim(anim_Walk, true);
            agent.speed = normalSpeed;

            yield return new WaitForSeconds(cooldownTime);
            if (isDead) yield break;

            // WINDUP — dur ve şarj ol
            currentState = State.Windup;
            SetColor(windupColor);
            SetAnim(anim_Walk, false);
            SetAnim(anim_Windup, true);
            agent.isStopped = true;
            PlayClip(windupSound);
            Debug.Log("[KurmalıZombie] ŞARJ OLUYOR...");

            yield return new WaitForSeconds(windupTime);
            if (isDead) yield break;

            SetAnim(anim_Windup, false);

            // SPRINT
            currentState = State.Sprint;
            SetColor(sprintColor);
            agent.speed     = normalSpeed * sprintMul;
            agent.isStopped = false;
            TriggerAnim(anim_Sprint);
            PlayClip(sprintSound);
            Debug.Log("[KurmalıZombie] SPRINT!");

            yield return new WaitForSeconds(sprintTime);
        }
    }

    // ── Update ────────────────────────────────────────────────────────────

    void Update()
    {
        if (isDead || player == null) return;
        if (!agent.isOnNavMesh) return;

        float dist = Vector3.Distance(
            new Vector3(transform.position.x, 0, transform.position.z),
            new Vector3(player.position.x,    0, player.position.z));

        // Her durumda hedefe yönel (windup hariç)
        if (currentState != State.Windup)
            agent.SetDestination(player.position);

        // Saldırı menzilindeyse vur
        if (dist <= attackRange && Time.time >= nextAttackTime)
        {
            DoAttack();
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    void DoAttack()
    {
        TriggerAnim(anim_Attack);
        player.GetComponent<IDamageable>()?.TakeDamage(attackDamage);
    }

    // ── IDamageable ───────────────────────────────────────────────────────

    public void TakeDamage(float amount)
    {
        if (isDead) return;
        currentHealth -= amount;
        currentHealth  = Mathf.Max(0f, currentHealth);

        GetComponent<ZombieHitFlash>()?.Flash();
        GetComponent<ZombieHitReact>()?.React(
            Camera.main != null ? Camera.main.transform.position : transform.position);

        if (currentHealth <= 0f) Die();
    }

    // ── Death ─────────────────────────────────────────────────────────────

    void Die()
    {
        if (isDead) return;
        isDead = true;

        StopAllCoroutines();
        GameEvents.FireEnemyKilled(gameObject);
        PlayClip(deathSound);
        TriggerAnim(anim_Die);

        agent.isStopped = true;
        agent.enabled   = false;

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

    void TriggerAnim(string trigger)
    {
        if (animator == null || animator.runtimeAnimatorController == null) return;
        if (string.IsNullOrEmpty(trigger)) return;
        animator.SetTrigger(trigger);
    }

    void SetAnim(string name, bool value)
    {
        if (animator == null || animator.runtimeAnimatorController == null) return;
        if (string.IsNullOrEmpty(name)) return;
        // Bool veya trigger olabilir — önce bool dene
        animator.SetBool(name, value);
    }

    void PlayClip(AudioClip clip)
    {
        if (clip == null || audioSource == null) return;
        audioSource.PlayOneShot(clip);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
