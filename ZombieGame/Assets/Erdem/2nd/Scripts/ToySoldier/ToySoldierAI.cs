using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class ToySoldierAI : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Ranges")]
    public float chaseRange      = 40f;
    public float firingRange     = 15f;   // bu mesafede dur ve ates et
    public float minRange        = 6f;    // bu mesafeden yakin → geri cekil

    [Header("Movement")]
    public float moveSpeed  = 3.5f;
    public float turnSpeed  = 10f;

    [Header("Animation")]
    public Animator animator;

    [Header("Visual Rotation")]
    [Tooltip("Mixamo modeller genellikle 180 offset ister.")]
    public Transform visual;
    public float visualYawOffset = 180f;

    private NavMeshAgent agent;
    private bool isFiring;

    // ── Lifecycle ──────────────────────────────────────────────────────────

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed         = moveSpeed;
        agent.autoBraking   = true;
        agent.stoppingDistance = Mathf.Max(0f, firingRange - 1f);

        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (visual == null && transform.childCount > 0) visual = transform.GetChild(0);
    }

    void Start()
    {
        if (target == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) target = p.transform;
        }
    }

    void Update()
    {
        if (target == null || !agent.isOnNavMesh) return;

        Vector3 myFlat  = new Vector3(transform.position.x, 0f, transform.position.z);
        Vector3 tFlat   = new Vector3(target.position.x,    0f, target.position.z);
        float   dist    = Vector3.Distance(myFlat, tFlat);

        if (dist > chaseRange)
        {
            StopAgent();
            SetFiring(false);
            return;
        }

        if (dist >= minRange && dist <= firingRange)
        {
            // Ideal ates mesafesinde: dur, ates et
            StopAgent();
            SetFiring(true);
            FaceTarget();
        }
        else if (dist < minRange)
        {
            // Cok yakin: geri cekil
            SetFiring(false);
            Retreat();
        }
        else
        {
            // Cok uzak: kos
            SetFiring(false);
            ChasePlayer();
        }
    }

    // ── Movement ──────────────────────────────────────────────────────────

    void ChasePlayer()
    {
        agent.isStopped        = false;
        agent.stoppingDistance = Mathf.Max(0f, firingRange - 1f);
        agent.SetDestination(target.position);
        RotateVisualToVelocity();
    }

    void Retreat()
    {
        agent.isStopped        = false;
        agent.stoppingDistance = 0f;
        Vector3 awayDir = (transform.position - target.position).normalized;
        agent.SetDestination(transform.position + awayDir * (minRange + 3f));
        RotateVisualToVelocity();
    }

    void StopAgent()
    {
        if (!agent.isStopped)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }

    // ── Firing flag ───────────────────────────────────────────────────────

    void SetFiring(bool value)
    {
        if (isFiring == value) return;
        isFiring = value;

        if (animator != null && animator.runtimeAnimatorController != null)
            animator.SetBool("IsFiring", isFiring);

        GetComponent<ToySoldierShooter>()?.SetFiring(isFiring);
    }

    // ── Rotation ──────────────────────────────────────────────────────────

    void FaceTarget()
    {
        if (visual == null || target == null) return;
        Vector3 dir = target.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) return;
        Quaternion goal = Quaternion.LookRotation(dir.normalized) * Quaternion.Euler(0f, visualYawOffset, 0f);
        visual.rotation = Quaternion.Slerp(visual.rotation, goal, turnSpeed * Time.deltaTime);
    }

    void RotateVisualToVelocity()
    {
        if (visual == null) return;
        Vector3 v = agent.velocity; v.y = 0f;
        if (v.sqrMagnitude > 0.01f)
        {
            Quaternion goal = Quaternion.LookRotation(v.normalized) * Quaternion.Euler(0f, visualYawOffset, 0f);
            visual.rotation = Quaternion.Slerp(visual.rotation, goal, turnSpeed * Time.deltaTime);
        }
    }

    // ── Called by ToySoldierHealth on death ───────────────────────────────

    public void OnDeath()
    {
        enabled = false;
        if (agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
        agent.enabled = false;
    }
}
