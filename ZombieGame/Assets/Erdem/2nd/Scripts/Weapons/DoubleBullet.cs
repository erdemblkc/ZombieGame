using UnityEngine;

public class DoubleBullet : MonoBehaviour
{
    [Header("Spawn")]
    public Bullet bulletPrefab;
    public float spawnForwardOffset = 0.03f;

    [Header("Two Barrels (optional)")]
    public Transform leftBarrel;
    public Transform rightBarrel;

    [Header("Fallback if no barrels")]
    public float sideOffset = 0.06f; // sa�-sol mesafe (barrel yoksa)

    [Header("Stats")]
    public float damage = 25f;

    /// <summary>Set by GunShooter when PiercerUpgrade is active.</summary>
    [HideInInspector] public int piercingCount = 0;
    /// <summary>Set by GunShooter when RicochetUpgrade is active.</summary>
    [HideInInspector] public int ricochetCount = 0;

    public void Fire(Vector3 dir)
    {
        if (bulletPrefab == null) return;

        // 1) E�er iki barrel transformun varsa onlar� kullan
        if (leftBarrel != null && rightBarrel != null)
        {
            SpawnFrom(leftBarrel, dir);
            SpawnFrom(rightBarrel, dir);
        }
        else
        {
            // 2) Barrel yoksa: tek noktadan sa�/sol offset ile spawn et
            Vector3 basePos = transform.position + transform.forward * spawnForwardOffset;
            Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);

            SpawnAt(basePos + transform.right * (-sideOffset * 0.5f), rot, dir);
            SpawnAt(basePos + transform.right * (sideOffset * 0.5f), rot, dir);
        }

        // Bu parent objeye art�k gerek yok
        Destroy(gameObject);
    }

    void SpawnFrom(Transform barrel, Vector3 dir)
    {
        Vector3 pos = barrel.position + barrel.forward * spawnForwardOffset;
        Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);
        SpawnAt(pos, rot, dir);
    }

    void SpawnAt(Vector3 pos, Quaternion rot, Vector3 dir)
    {
        Bullet b = Instantiate(bulletPrefab, pos, rot);
        b.damage        = damage;
        b.piercingCount = piercingCount;
        b.ricochetCount = ricochetCount;
        b.Fire(dir);
    }
}
