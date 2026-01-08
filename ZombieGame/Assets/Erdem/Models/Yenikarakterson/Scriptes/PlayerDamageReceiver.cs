using UnityEngine;

[RequireComponent(typeof(InfectionSystem))]
public class PlayerDamageReceiver : MonoBehaviour
{
    [Header("Health (optional)")]
    public float maxHealth = 100f;
    public float currentHealth = 100f;

    [Header("Damage Cooldown")]
    public float damageCooldown = 0.1f;

    [Header("Knockback (Minecraft-ish)")]
    public bool enableKnockback = true;
    public float knockbackHorizontal = 6f;
    public float knockbackUp = 2.2f;
    public float knockbackDuration = 0.12f;

    [Header("Debug")]
    public bool debugLogs = false;

    private float _lastDamageTime = -999f;
    private InfectionSystem _infection;
    private CharacterController _cc;

    private Vector3 _knockVel;
    private float _knockTimeLeft;

    void Awake()
    {
        _infection = GetComponent<InfectionSystem>();
        _cc = GetComponent<CharacterController>();
    }

    void Start()
    {
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
    }

    void Update()
    {
        if (!enableKnockback) return;

        if (_knockTimeLeft > 0f)
        {
            _knockTimeLeft -= Time.deltaTime;

            float t = (_knockTimeLeft / Mathf.Max(0.0001f, knockbackDuration));
            Vector3 v = _knockVel * t;

            if (_cc != null) _cc.Move(v * Time.deltaTime);
            else transform.position += v * Time.deltaTime;
        }
    }

    // Zombie bunu çaðýrýyor
    public void TakeDamage(float damageAmount)
    {
        if (Time.time - _lastDamageTime < damageCooldown) return;
        _lastDamageTime = Time.time;

        currentHealth = Mathf.Clamp(currentHealth - damageAmount, 0f, maxHealth);

        // Her hasarda enfeksiyon +10
        _infection.OnPlayerDamaged();

        if (debugLogs)
            Debug.Log($"[{name}] Took {damageAmount} damage. HP: {currentHealth}/{maxHealth} | Infection: {_infection.infection}/{_infection.maxInfection}");

        if (currentHealth <= 0f)
        {
            Debug.Log("Player died (health).");
        }
    }

    // 0.14 sn'de vurulduðunda Zombie burayý çaðýracak
    public void ApplyKnockbackFrom(Vector3 attackerPosition)
    {
        if (!enableKnockback) return;

        Vector3 dir = (transform.position - attackerPosition);
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.0001f)
            dir = transform.forward; // çok üst üste ise fallback

        dir.Normalize();

        _knockVel = dir * knockbackHorizontal + Vector3.up * knockbackUp;
        _knockTimeLeft = knockbackDuration;
    }
}
