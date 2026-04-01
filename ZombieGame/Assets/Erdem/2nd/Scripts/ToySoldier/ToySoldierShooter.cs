using UnityEngine;

/// <summary>
/// ToySoldierAI tarafından SetFiring(true) çağrılınca periyodik mermi spawn eder.
/// </summary>
public class ToySoldierShooter : MonoBehaviour
{
    [Header("Bullet")]
    public GameObject bulletPrefab;
    public float      fireInterval = 1.8f;

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
    private float     nextFireTime;
    private Transform target;

    // ── Lifecycle ──────────────────────────────────────────────────────────

    void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) target = p.transform;
    }

    void Update()
    {
        if (!firing || target == null) return;
        if (Time.time < nextFireTime) return;

        Shoot();
        nextFireTime = Time.time + fireInterval;
    }

    // ── Public API ─────────────────────────────────────────────────────────

    public void SetFiring(bool value)
    {
        firing = value;
        if (value)
            nextFireTime = Time.time + 0.4f;
    }

    // ── Shooting ───────────────────────────────────────────────────────────

    void Shoot()
    {
        if (bulletPrefab == null) return;

        Transform origin = muzzlePoint != null ? muzzlePoint : transform;

        // Oyuncunun göğsüne doğru yönlen
        Vector3 dir = ((target.position + Vector3.up * 1.0f) - origin.position).normalized;

        // Saçılma uygula
        dir = Quaternion.Euler(
            Random.Range(-spreadAngle, spreadAngle),
            Random.Range(-spreadAngle, spreadAngle),
            0f) * dir;

        // Mermiyi spawn et
        GameObject bullet = Instantiate(bulletPrefab, origin.position, Quaternion.LookRotation(dir));
        var b = bullet.GetComponent<ToySoldierBullet>();
        if (b != null) b.damage = 10f;

        // Ses
        if (audioSource != null && fireSound != null)
            audioSource.PlayOneShot(fireSound);
    }
}
