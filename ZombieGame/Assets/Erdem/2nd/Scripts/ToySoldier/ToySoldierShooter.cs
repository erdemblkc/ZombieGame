using UnityEngine;

/// <summary>
/// ToySoldierAI tarafından SetFiring(true) çağrılınca ateş eder.
/// Animator'ın normalized time'ını takip eder — her animasyon loopunda 1 mermi atar.
/// Animation Event gerekmez.
/// </summary>
public class ToySoldierShooter : MonoBehaviour
{
    [Header("Bullet")]
    public GameObject bulletPrefab;

    [Header("Accuracy")]
    [Tooltip("Derece cinsinden maksimum saçılma.")]
    public float spreadAngle = 5f;

    [Header("Muzzle")]
    [Tooltip("Silahın namlu ucu. Boş bırakılırsa gövde merkezinden ateşler.")]
    public Transform muzzlePoint;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip   fireSound;

    private bool      firing;
    private Transform target;
    private Animator  animator;
    private float     lastNormalizedTime;
    private bool      hasFiredThisLoop;

    // Animator'da Firing Rifle state'inin adı
    private const string FiringStateName = "FiringRifle";

    // ── Lifecycle ──────────────────────────────────────────────────────────

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) target = p.transform;
    }

    void Update()
    {
        if (!firing || target == null || animator == null) return;

        var stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        // Firing Rifle state'inde değilsek bir şey yapma
        if (!stateInfo.IsName(FiringStateName)) return;

        float normalizedTime = stateInfo.normalizedTime % 1f;

        // Loop başına gelince (normalizedTime sıfırlandı) ateş et
        if (normalizedTime < lastNormalizedTime)
        {
            hasFiredThisLoop = false;
        }

        // Her loop'ta bir kez, animasyonun ilk %20'sinde ateş et
        if (!hasFiredThisLoop && normalizedTime >= 0.05f && normalizedTime < 0.2f)
        {
            Shoot();
            hasFiredThisLoop = true;
        }

        lastNormalizedTime = normalizedTime;
    }

    // ── Public API ─────────────────────────────────────────────────────────

    public void SetFiring(bool value)
    {
        firing = value;
        if (!value)
        {
            hasFiredThisLoop   = false;
            lastNormalizedTime = 0f;
        }
    }

    // ── Shooting ───────────────────────────────────────────────────────────

    void Shoot()
    {
        if (bulletPrefab == null || target == null) return;

        Transform origin = muzzlePoint != null ? muzzlePoint : transform;

        Vector3 dir = ((target.position + Vector3.up * 1.0f) - origin.position).normalized;

        dir = Quaternion.Euler(
            Random.Range(-spreadAngle, spreadAngle),
            Random.Range(-spreadAngle, spreadAngle),
            0f) * dir;

        Instantiate(bulletPrefab, origin.position, Quaternion.LookRotation(dir));

        if (audioSource != null && fireSound != null)
            audioSource.PlayOneShot(fireSound);
    }
}
