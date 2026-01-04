using UnityEngine;

public class GunShooter : MonoBehaviour
{
    [Header("Refs")]
    public Animator anim;          // player'ýn animator'ý
    public Transform cameraPivot;  // kamera / pivot
    public Transform muzzle;       // silah ucu
    public Bullet bulletPrefab;    // Bullet.prefab

    [Header("Settings")]
    public string aimingParam = "IsAiming";
    public string shootParam = "Shoot";
    public float damage = 25f;
    public float fireCooldown = 0.12f;
    public float bulletSpawnOffset = 0.03f;

    float nextFireTime;

    void Awake()
    {
        // otomatik bulmaya çalýþalým (istersen inspector'dan da verirsin)
        if (anim == null)
        {
            anim = GetComponentInChildren<Animator>();
            if (anim == null) anim = GetComponent<Animator>();
        }

        if (cameraPivot == null && Camera.main != null)
            cameraPivot = Camera.main.transform;
    }

    void Update()
    {
        // Aim: sað týk
        bool isAiming = Input.GetMouseButton(1);
        if (anim != null) anim.SetBool(aimingParam, isAiming);

        // Shoot: sol týk (cooldown)
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireCooldown;

            if (anim != null) anim.SetTrigger(shootParam);

            ShootProjectile();
        }
    }

    void ShootProjectile()
    {
        if (bulletPrefab == null || muzzle == null || cameraPivot == null) return;

        Vector3 dir = cameraPivot.forward;

        Vector3 spawnPos = muzzle.position + muzzle.forward * bulletSpawnOffset;
        Quaternion spawnRot = Quaternion.LookRotation(dir, Vector3.up);

        Bullet b = Instantiate(bulletPrefab, spawnPos, spawnRot);
        b.damage = damage;
        b.Fire(dir);
    }
}
