using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class ZombieAI_Follow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Ranges")]
    public float chaseRange = 50f;

    [Tooltip("Bu mesafenin içine girince dur + saldýr.")]
    public float attackRange = 1.8f;

    [Tooltip("Titremeyi önler: bu mesafenin üstüne çýkýnca tekrar kovalar.")]
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

    private NavMeshAgent agent;
    private bool inAttackZone;
    private float nextAttackTime;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.autoBraking = true;
        agent.stoppingDistance = 0f;

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (visual == null && transform.childCount > 0)
            visual = transform.GetChild(0);

        // Uyarý
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
    }

    void Update()
    {

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

            RotateVisualToVelocity();
        }
    }

    bool CanAnimate()
    {
        return animator != null && animator.runtimeAnimatorController != null;
    }

    void StopAgent()
    {
        agent.isStopped = true;
        agent.ResetPath();
    }

    void SetWalking(bool value)
    {
        if (CanAnimate())
            animator.SetBool(isWalkingBool, value);
    }

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
