using UnityEngine;

public class ZombieHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    private bool isDead = false;
    private ZombieFollowPlayer zombieFollow;
    public float destroyDelayAfterDeath = 3f;

    void Awake()
    {
        zombieFollow = GetComponent<ZombieFollowPlayer>();
    }

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        if (zombieFollow != null)
            zombieFollow.Die();

        Destroy(gameObject, destroyDelayAfterDeath);
    }
}
