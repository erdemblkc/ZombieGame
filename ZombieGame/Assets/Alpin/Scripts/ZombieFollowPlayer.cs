using UnityEngine;

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
    public Animator animator;           // Zombinin Animator'u
    public string speedParam = "Speed"; // Animator'daki float parametre adı

    private float nextAttackTime = 0f;

    private Transform target;
    private PlayerHealth playerHealth;

    void Start()
    {
        // Sahnedeki PlayerHealth'i bul ve hedef olarak onu kullan
        playerHealth = FindFirstObjectByType<PlayerHealth>();

        if (playerHealth != null)
        {
            target = playerHealth.transform;
        }
        else
        {
            Debug.LogWarning("Zombie: Sahne'de PlayerHealth bulunamadı!");
        }

        // Animatörü otomatik bul (Inspector'dan atamazsan)
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    void Update()
    {
        if (target == null) return;

        // Player'a yön ve mesafe
        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        float distance = direction.magnitude;

        if (distance > 0.01f)
            direction.Normalize();

        // Oyuncuya bak
        if (direction != Vector3.zero)
        {
            transform.forward = direction;
        }

        // Hareket ediyor mu?
        bool isMoving = false;

        // Eğer çok uzaktaysa yürüsün
        if (distance > stopDistance)
        {
            transform.position += direction * moveSpeed * Time.deltaTime;
            isMoving = true;
        }

        // 🔹 Animasyon: Yürüme / Idle
        if (animator != null)
        {
            float speed01 = isMoving ? 1f : 0f;
            animator.SetFloat(speedParam, speed01);
        }

        // 🔹 Saldırı
        if (distance <= attackRange && playerHealth != null)
        {
            if (Time.time >= nextAttackTime)
            {
                nextAttackTime = Time.time + 1f / attackRate;

                // Zombiden player'a doğru vektör
                Vector3 hitDir = (target.position - transform.position).normalized;

                // Hasar + knockback
                playerHealth.TakeDamage(attackDamage, hitDir);
            }
        }
    }

    // Editör'de menzilleri görmek için (isteğe bağlı)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stopDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
