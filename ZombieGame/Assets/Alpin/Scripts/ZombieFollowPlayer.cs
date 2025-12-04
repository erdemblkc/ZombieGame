using UnityEngine;
using UnityEngine.AI;

public class ZombieFollowPlayer : MonoBehaviour
{
    [Header("Hareket")]
    public float moveSpeed = 3f;        // Zombi hızı
    public float stopDistance = 1.3f;   // Bu mesafeye kadar yürüsün

    [Header("Saldırı")]
    public float attackRange = 1.6f;    // Bu mesafedeyken vurabilsin
    public float attackDamage = 10f;    // Her vuruşta vereceği hasar
    public float attackRate = 1f;       // Saniyede kaç kere vurabilir (1 = 1/sn)

    [Header("Animasyon")]
    public Animator animator;           // Model Mesh üzerindeki Animator

    // Animator state isimleri (Animator penceresindeki state adlarıyla birebir aynı olmalı!)
    private const string IdleStateName = "Idle";
    private const string WalkStateName = "walk";

    private float nextAttackTime = 0f;

    private Transform target;
    private PlayerHealth playerHealth;
    private NavMeshAgent agent;

    private bool lastIsMoving = false;

    void Start()
    {
        // Player'ı bul
        playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (playerHealth != null)
        {
            target = playerHealth.transform;
        }
        else
        {
            Debug.LogWarning("Zombie: Sahne'de PlayerHealth bulunamadı!");
        }

        // NavMeshAgent'i al
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("Zombie: NavMeshAgent component'i yok! " + name);
        }
        else
        {
            agent.speed = moveSpeed;
            agent.stoppingDistance = stopDistance;
            agent.updateRotation = true;
            agent.updatePosition = true;
        }

        // Animatörü al: inspector'dan atanmamışsa child'lardan bul
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (animator == null)
        {
            Debug.LogError("Zombie: Animator bulunamadı! " + name);
        }
        else
        {
            // Başlangıçta Idle'a geç
            animator.Play(IdleStateName, 0, 0f);
        }
    }

    void Update()
    {
        if (agent == null || target == null) return;

        // Her frame hedef olarak player'ı ver
        agent.SetDestination(target.position);

        float distance = Vector3.Distance(transform.position, target.position);

        // NavMeshAgent hızına göre "yürüyor mu?"
        bool isMoving = agent.velocity.sqrMagnitude > 0.01f;

        // Animasyon: sadece durum değiştiğinde Idle <-> walk arasında geçiş yap
        if (animator != null && isMoving != lastIsMoving)
        {
            if (isMoving)
            {
                animator.CrossFade(WalkStateName, 0.1f);
            }
            else
            {
                animator.CrossFade(IdleStateName, 0.1f);
            }

            lastIsMoving = isMoving;
        }

        // Saldırı
        if (distance <= attackRange && playerHealth != null)
        {
            if (Time.time >= nextAttackTime)
            {
                nextAttackTime = Time.time + 1f / attackRate;

                // Zombiden player'a doğru vektör
                Vector3 hitDir = (target.position - transform.position).normalized;
                playerHealth.TakeDamage(attackDamage, hitDir);
            }
        }
    }

    // Editör'de menzilleri görmek için
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stopDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
