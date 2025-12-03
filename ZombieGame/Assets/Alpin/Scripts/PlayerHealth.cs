using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerHealth : MonoBehaviour
{
    [Header("Can Ayarlarý")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Knockback Ayarlarý")]
    public float knockbackForce = 18f;      // Ne kadar sert itilsin
    public float knockbackUpForce = 1f;     // Ne kadar havaya kalksýn

    private PlayerMovement playerMovement;

    void Start()
    {
        currentHealth = maxHealth;
        playerMovement = GetComponent<PlayerMovement>();
    }

    /// <summary>
    /// Yönlü hasar (zombiden vs.)
    /// hitDirection: vurulan yön (zombiden player'a doðru vektör)
    /// </summary>
    public void TakeDamage(float amount, Vector3 hitDirection)
    {
        currentHealth -= amount;
        Debug.Log("Hasar aldýk: " + amount + "  Güncel can: " + currentHealth);

        // Knockback uygula
        if (playerMovement != null)
        {
            playerMovement.AddImpact(hitDirection, knockbackForce, knockbackUpForce);
        }

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    /// <summary>
    /// Yön vermeden hasar (test amaçlý)
    /// Player'ýn baktýðý yönün tersine (arkaya) knockback uygular
    /// </summary>
    public void TakeDamage(float amount)
    {
        // -transform.forward = arkaya doðru
        TakeDamage(amount, -transform.forward);
    }

    void Die()
    {
        Debug.Log("Player öldü!");
        // Ýleride restart / animasyon vs.
    }

    // Test: H tuþuna basýnca 10 damage + knockback
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            TakeDamage(10f);
        }
    }
}
