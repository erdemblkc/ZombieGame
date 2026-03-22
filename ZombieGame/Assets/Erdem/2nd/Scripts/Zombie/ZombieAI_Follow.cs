using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class ZombieAI_Follow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Ranges")]
    public float chaseRange = 50f;
    public float attackRange = 1.8f;
    public float resumeChaseRange = 2.3f;

    [Header("Attack")]
    public float attackCooldown = 1.2f;

    [Header("Animation")]
    public Animator animator;
    public string isWalkingBool = "IsWalking";
    public string attackTrigger = "Attack";

    [Header("Visual")]
    public Transform visual;
    public float turnSpeed = 12f;
    public float visualYawOffset = 180f;

    [Header("Zombie Audio (Only Idle & Step)")]
    public AudioSource audioSource;
    // public AudioClip attackSound; // <--- SÝLDÝK (Diðer scripte taþýdýk)
    public AudioClip[] idleSounds;
    public AudioClip[] stepSounds;

    [Header("Audio Settings")]
    public float idleSoundIntervalMin = 3f;
    public float idleSoundIntervalMax = 7f;
    public float stepInterval = 0.5f;

    private NavMeshAgent agent;
    private bool inAttackZone;
    private float nextAttackTime;

    private float nextIdleTime;
    private float stepTimer;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.autoBraking = true;
        agent.stoppingDistance = 0f;

        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (visual == null && transform.childCount > 0) visual = transform.GetChild(0);
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        if (GetComponent<ZombieAttackDamageTimed>() == null)
            Debug.LogWarning($"[{name}] ZombieAttackDamageTimed component is missing on this zombie!");
    }

    void Start()
    {
        if (target == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) target = p.transform;
        }
        nextIdleTime = Time.time + Random.Range(0f, 2f);
    }

    void Update()
    {
        // --- IDLE SOUND ---
        if (Time.time >= nextIdleTime && !inAttackZone)
        {
            PlayRandomIdleSound();
            nextIdleTime = Time.time + Random.Range(idleSoundIntervalMin, idleSoundIntervalMax);
        }

        if (target == null) return;
        if (!agent.isOnNavMesh) return;

        Vector3 a = transform.position; a.y = 0f;
        Vector3 b = target.position; b.y = 0f;
        float d = Vector3.Distance(a, b);

        if (d > chaseRange)
        {
            StopAgent();
            SetWalking(false);
            return;
        }

        if (!inAttackZone && d <= attackRange) inAttackZone = true;
        if (inAttackZone && d >= resumeChaseRange) inAttackZone = false;

        if (inAttackZone)
        {
            StopAgent();
            SetWalking(false);

            if (CanAnimate() && Time.time >= nextAttackTime)
            {
                animator.ResetTrigger(attackTrigger);
                animator.SetTrigger(attackTrigger);

                // --- BURADAKÝ SES KODUNU KALDIRDIK ---
                // Sadece mekanik saldýrýyý baþlatýyoruz:
                GetComponent<ZombieAttackDamageTimed>()?.DoAttack();

                nextAttackTime = Time.time + attackCooldown;
            }
        }
        else
        {
            agent.isStopped = false;
            agent.SetDestination(target.position);
            bool walking = agent.velocity.sqrMagnitude > 0.05f;
            SetWalking(walking);

            if (walking)
            {
                stepTimer -= Time.deltaTime;
                if (stepTimer <= 0f)
                {
                    PlayStepSound();
                    stepTimer = stepInterval;
                }
            }
            RotateVisualToVelocity();
        }
    }

    void PlayRandomIdleSound()
    {
        if (audioSource == null || idleSounds == null || idleSounds.Length == 0) return;
        if (audioSource.isPlaying) return;

        AudioClip clip = idleSounds[Random.Range(0, idleSounds.Length)];
        audioSource.pitch = Random.Range(0.8f, 1.0f);
        audioSource.PlayOneShot(clip);
    }

    void PlayStepSound()
    {
        if (audioSource == null || stepSounds == null || stepSounds.Length == 0) return;
        AudioClip clip = stepSounds[Random.Range(0, stepSounds.Length)];
        audioSource.PlayOneShot(clip, 0.4f);
    }

    bool CanAnimate() { return animator != null && animator.runtimeAnimatorController != null; }
    void StopAgent() { agent.isStopped = true; agent.ResetPath(); }
    void SetWalking(bool value) { if (CanAnimate()) animator.SetBool(isWalkingBool, value); }
    void RotateVisualToVelocity()
    {
        if (visual == null) return;
        Vector3 v = agent.velocity;
        v.y = 0f;
        if (v.sqrMagnitude > 0.01f)
        {
            Quaternion lookRot = Quaternion.LookRotation(v.normalized, Vector3.up);
            lookRot *= Quaternion.Euler(0f, visualYawOffset, 0f);
            visual.rotation = Quaternion.Slerp(visual.rotation, lookRot, turnSpeed * Time.deltaTime);
        }
    }
}