using UnityEngine;
using System.Collections;

[RequireComponent(typeof(InfectionSystem))]
public class PlayerDamageReceiver : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
    public float currentHealth = 100f;

    [Header("Damage Cooldown")]
    public float damageCooldown = 0.1f;

    [Header("Knockback")]
    public bool enableKnockback = true;
    public float knockbackHorizontal = 6f;
    public float knockbackUp = 2.2f;
    public float knockbackDuration = 0.12f;

    [Header("Armor")]
    /// <summary>
    /// Multiplier applied to all incoming damage. ArmorUpgrade sets this to 0.85f.
    /// Stack multiplicatively if multiple armor sources exist.
    /// </summary>
    [HideInInspector] public float armorDamageMultiplier = 1f;

    [Header("Death Settings")]
    public Transform cameraPivot; // Player'�n kameras�n� ta��yan obje
    public MonoBehaviour[] scriptsToDisable; // �l�nce kapanacak scriptler (Controller, Shooter vb.)

    private float _lastDamageTime = -999f;
    private InfectionSystem _infection;
    private CharacterController _cc;
    private GameOverManager _gameOverManager;

    private Vector3 _knockVel;
    private float _knockTimeLeft;
    private bool _isDead = false;

    void Awake()
    {
        _infection = GetComponent<InfectionSystem>();
        _cc = GetComponent<CharacterController>();
        _gameOverManager = FindFirstObjectByType<GameOverManager>();
    }

    void Start()
    {
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
    }

    void Update()
    {
        if (_isDead) return;

        if (!enableKnockback) return;

        if (_knockTimeLeft > 0f)
        {
            _knockTimeLeft -= Time.deltaTime;
            float t = (_knockTimeLeft / Mathf.Max(0.0001f, knockbackDuration));
            Vector3 v = _knockVel * t;

            if (_cc != null) _cc.Move(v * Time.deltaTime);
        }
    }

    public void TakeDamage(float damageAmount)
    {
        if (_isDead) return;
        if (Time.time - _lastDamageTime < damageCooldown) return;

        _lastDamageTime = Time.time;
        float effectiveDamage = damageAmount * armorDamageMultiplier;
        currentHealth = Mathf.Clamp(currentHealth - effectiveDamage, 0f, maxHealth);
        _infection.OnPlayerDamaged();

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        if (_isDead) return;
        _isDead = true;

        Debug.Log("Player Died!");

        // 1. Kontrolleri kapat
        if (scriptsToDisable != null)
        {
            foreach (var script in scriptsToDisable)
            {
                if (script != null) script.enabled = false;
            }
        }

        // 2. Kameray� �evir (Yere d��me hissi)
        StartCoroutine(DeathCameraRoutine());

        // 3. UI G�ster
        if (_gameOverManager != null)
        {
            _gameOverManager.ShowDeathScreen();
        }
    }

    IEnumerator DeathCameraRoutine()
    {
        if (cameraPivot == null) yield break;

        Quaternion startRot = cameraPivot.localRotation;
        Quaternion targetRot = Quaternion.Euler(-60f, 0f, 20f); // Tavana bak��

        float t = 0f;
        while (t < 1.5f)
        {
            t += Time.deltaTime;
            cameraPivot.localRotation = Quaternion.Slerp(startRot, targetRot, t * 2f);

            // Hafif�e yere ��k
            transform.position -= Vector3.up * Time.deltaTime * 0.5f;

            yield return null;
        }
    }

    public void ApplyKnockbackFrom(Vector3 attackerPosition)
    {
        if (_isDead || !enableKnockback) return;

        Vector3 dir = (transform.position - attackerPosition);
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) dir = transform.forward;
        dir.Normalize();

        _knockVel = dir * knockbackHorizontal + Vector3.up * knockbackUp;
        _knockTimeLeft = knockbackDuration;
    }

    public void FullHeal()
    {
        currentHealth = maxHealth;
    }

    /// <summary>Enfeksiyon gibi dış sistemler tarafından ölümü tetiklemek için kullanılır.</summary>
    public void ForceKill()
    {
        Die();
    }
}