using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class ZombieFollowPlayer : MonoBehaviour
{
    [Header("Target / Saldırı")]
    public Transform target;             // Player
    public float stopDistance = 1.5f;    // ne kadar yakında dursun
    public float attackRange = 1.8f;     // bu mesafede vurur
    public float timeBetweenAttacks = 1.0f;
    public float attackDamage = 10f;

    [Header("Animasyon")]
    public Animator animator;
    private const string WalkStateName = "Zombie|Walk";
    private const string AttackStateName = "Zombie|Attack";
    private const string DieStateName = "Zombie|Die";

    private NavMeshAgent agent;
    private float lastAttackTime = -999f;
    private bool isDead = false;
    private string currentState = "";

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (target == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                target = playerObj.transform;
        }

        if (target == null)
        {
            Debug.LogError("ZombieFollowPlayer: Target bulunamadı. Player tag i var mi?");
        }

        if (target != null)
        {
            agent.stoppingDistance = stopDistance;
            agent.isStopped = false;
            agent.SetDestination(target.position);
        }

        PlayState(WalkStateName); // spawn olur olmaz yürüsün
    }

    void Update()
    {
        if (isDead)
        {
            if (agent.enabled)
                agent.isStopped = true;
            return;
        }

        if (target == null)
            return;

        // her framede hedefe koş
        agent.isStopped = false;
        agent.SetDestination(target.position);

        float distance = Vector3.Distance(transform.position, target.position);

        // debug icin
        Debug.DrawLine(transform.position + Vector3.up * 0.5f, target.position + Vector3.up * 0.5f, Color.green);

        bool canAttack = distance <= attackRange &&
                         Time.time >= lastAttackTime + timeBetweenAttacks;

        if (canAttack)
        {
            StartAttack(distance);
            lastAttackTime = Time.time;
        }
    }

    void StartAttack(float distance)
    {
        Debug.Log("Zombie " + name + " attack deniyor. Mesafe = " + distance);

        PlayState(AttackStateName);

        // PlayerHealth bul: önce target'ın kendisinde, sonra parent'ında, sonra child'larında
        PlayerHealth ph = target.GetComponent<PlayerHealth>();
        if (ph == null) ph = target.GetComponentInParent<PlayerHealth>();
        if (ph == null) ph = target.GetComponentInChildren<PlayerHealth>();

        if (ph != null)
        {
            Debug.Log("Zombie " + name + " hasar verdi: " + attackDamage);
            ph.TakeDamage(attackDamage);
        }
        else
        {
            Debug.LogWarning("Zombie " + name + ": PlayerHealth hiçbir yerde bulunamadı (self/parent/child).");
        }

        // Knockback (istersen)
        PlayerMovement pm = target.GetComponent<PlayerMovement>();
        if (pm == null) pm = target.GetComponentInParent<PlayerMovement>();
        if (pm == null) pm = target.GetComponentInChildren<PlayerMovement>();

        if (pm != null)
        {
            Vector3 dir = (target.position - transform.position).normalized;
            pm.AddImpact(dir, 5f);
        }

        // Attack animasyonu bittikten sonra tekrar yürümeye dön
        Invoke(nameof(BackToWalk), 0.6f); // attack klibinin süresine göre ayarla
    }


    void BackToWalk()
    {
        if (isDead) return;
        PlayState(WalkStateName);
    }

    void PlayState(string stateName)
    {
        if (animator == null) return;
        if (currentState == stateName) return;

        animator.CrossFade(stateName, 0.1f);
        currentState = stateName;
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        PlayState(DieStateName);

        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = false;
    }

    // Sahnedeki attackRange i görmek için
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
