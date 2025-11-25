using UnityEngine;

public class ZombieHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        // Ýleride burada ölüm animasyonu, ses, loot vs. oynatabiliriz
        Destroy(gameObject);
    }
}
