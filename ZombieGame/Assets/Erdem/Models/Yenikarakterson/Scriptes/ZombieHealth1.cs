using UnityEngine;

public class ZombieHealth1 : MonoBehaviour, IDamageable
{
    [Header("Health")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Death")]
    public bool destroyOnDeath = true;
    public float destroyDelay = 2f;

    private Animator anim;
    private bool isDead;

    void Awake()
    {
        currentHealth = maxHealth;
        anim = GetComponentInChildren<Animator>();
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        Debug.Log($"{name} took {amount} damage. HP: {currentHealth}");

        if (currentHealth <= 0f)
            Die();
    }

    void Die()
    {
        isDead = true;

        Debug.Log($"{name} DIED!");

        // Collider’larý kapat ki artýk mermi çarpmasýn
        foreach (var col in GetComponentsInChildren<Collider>())
            col.enabled = false;

        // Ýleride anim ekleyeceðiz (þimdilik sadece destroy)
        if (destroyOnDeath)
            Destroy(gameObject, destroyDelay);
    }
}
