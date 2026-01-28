using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class GunShooter : MonoBehaviour
{
    [Header("Refs")]
    public Animator anim;
    public Transform cameraPivot;
    public Transform muzzle;
    public Bullet bulletPrefab;
    public TextMeshProUGUI ammoText;

    [Header("Alt Fire (New Gun)")]
    public bool useDoubleBulletPrefab = false;   // upgrade olunca true
    public DoubleBullet doubleBulletPrefab;      // DoubleBullet prefab

    [Header("New Gun Barrels (Optional)")]
    public Transform leftBarrel;
    public Transform rightBarrel;

    [Header("Reload UI")]
    public Image reloadCircle;          // Radial image (fill)
    public bool showReloadCircle = true;

    [Header("Animator Params")]
    public string aimingParam = "IsAiming";
    public string shootParam = "Shoot";

    [Header("Reload Idle Override")]
    public string idleStateName = "Stand";
    public float idleForceBlend = 0.05f;

    [Header("Gun Settings")]
    public float fireCooldown = 0.12f;
    public float bulletSpawnOffset = 0.03f;

    [Header("Weapon Damage")]
    public float oldGunDamage = 25f;
    public float newGunDamage = 100f;

    [Header("Ammo (OLD GUN)")]
    public int oldMagazineSize = 12;
    public int oldReserveStart = 36;
    public float oldReloadTime = 1.0f;

    [Header("Ammo (NEW GUN)")]
    public int newMagazineSize = 2;     // istediğin: 2
    public int newReserveStart = 12;    // istediğin: 12
    public float newReloadTime = 1.8f;  // yeni silah reload daha uzun olsun istersen 1.8-3.0 arası

    [Header("Ammo General")]
    public bool autoReloadWhenEmpty = true;

    // ---- runtime ----
    int currentAmmo;
    int reserveAmmo;
    int magazineSize;
    float reloadTime;

    float nextFireTime;
    bool isReloading;

    // İki silahın ammo havuzları ayrı
    int old_currentAmmo, old_reserveAmmo;
    int new_currentAmmo, new_reserveAmmo;

    bool lastWeaponMode;

    void Awake()
    {
        if (anim == null)
        {
            anim = GetComponentInChildren<Animator>();
            if (anim == null) anim = GetComponent<Animator>();
        }

        if (cameraPivot == null && Camera.main != null)
            cameraPivot = Camera.main.transform;

        // Başlangıç ammo havuzları
        old_currentAmmo = oldMagazineSize;
        old_reserveAmmo = oldReserveStart;

        new_currentAmmo = newMagazineSize;
        new_reserveAmmo = newReserveStart;

        lastWeaponMode = useDoubleBulletPrefab;

        ApplyWeaponMode(force: true);

        UpdateAmmoUI();
        SetReloadCircleVisible(false);
    }

    void Update()
    {
        // Silah modu değiştiyse (upgrade sonrası) yeni ayarları uygula
        if (useDoubleBulletPrefab != lastWeaponMode)
        {
            ApplyWeaponMode(force: true);
        }

        // Reload sırasında: shoot anim bastır + idle'a zorla
        if (isReloading)
        {
            if (anim != null)
            {
                anim.ResetTrigger(shootParam);
                if (!string.IsNullOrEmpty(idleStateName))
                    anim.CrossFade(idleStateName, idleForceBlend, 0);
            }
            return;
        }

        // Reload: R
        if (Input.GetKeyDown(KeyCode.R))
        {
            TryReload();
            return;
        }

        // boşsa otomatik reload
        if (currentAmmo <= 0)
        {
            if (autoReloadWhenEmpty) TryReload();
            return;
        }

        // Shoot: sol tık (cooldown)
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            if (currentAmmo <= 0) return;

            nextFireTime = Time.time + fireCooldown;

            currentAmmo--;
            SyncAmmoToPool();
            UpdateAmmoUI();

            if (anim != null) anim.SetTrigger(shootParam);
            ShootProjectile();
        }
    }

    // Silah modu değişince: doğru mag/reserve/reloadTime yükle + UI güncelle
    void ApplyWeaponMode(bool force)
    {
        lastWeaponMode = useDoubleBulletPrefab;

        if (useDoubleBulletPrefab)
        {
            // NEW GUN mode
            magazineSize = newMagazineSize;
            reloadTime = newReloadTime;

            currentAmmo = new_currentAmmo;
            reserveAmmo = new_reserveAmmo;
        }
        else
        {
            // OLD GUN mode
            magazineSize = oldMagazineSize;
            reloadTime = oldReloadTime;

            currentAmmo = old_currentAmmo;
            reserveAmmo = old_reserveAmmo;
        }

        UpdateAmmoUI();
    }

    // currentAmmo/reserveAmmo değişince doğru havuza yaz
    void SyncAmmoToPool()
    {
        if (useDoubleBulletPrefab)
        {
            new_currentAmmo = currentAmmo;
            new_reserveAmmo = reserveAmmo;
        }
        else
        {
            old_currentAmmo = currentAmmo;
            old_reserveAmmo = reserveAmmo;
        }
    }

    void TryReload()
    {
        if (isReloading) return;
        if (currentAmmo >= magazineSize) return;
        if (reserveAmmo <= 0) return;

        StartCoroutine(ReloadRoutine());
    }

    IEnumerator ReloadRoutine()
    {
        isReloading = true;

        // Ateşi kes
        nextFireTime = Mathf.Max(nextFireTime, Time.time + reloadTime);

        if (showReloadCircle) SetReloadCircleVisible(true);

        float t = 0f;
        while (t < reloadTime)
        {
            t += Time.deltaTime;

            if (reloadCircle != null)
                reloadCircle.fillAmount = Mathf.Clamp01(t / reloadTime);

            if (anim != null)
            {
                anim.ResetTrigger(shootParam);
                if (!string.IsNullOrEmpty(idleStateName))
                    anim.CrossFade(idleStateName, idleForceBlend, 0);
            }

            yield return null;
        }

        int need = magazineSize - currentAmmo;
        int take = Mathf.Min(need, reserveAmmo);

        currentAmmo += take;
        reserveAmmo -= take;

        SyncAmmoToPool();
        UpdateAmmoUI();

        SetReloadCircleVisible(false);

        isReloading = false;
    }

    void ShootProjectile()
    {
        if (isReloading) return;
        if (muzzle == null || cameraPivot == null) return;

        // aktif silaha göre damage seç
        float dmg = useDoubleBulletPrefab ? newGunDamage : oldGunDamage;

        Vector3 dir = cameraPivot.forward;
        Vector3 spawnPos = muzzle.position + muzzle.forward * bulletSpawnOffset;
        Quaternion spawnRot = Quaternion.LookRotation(dir, Vector3.up);

        // NEW GUN: DoubleBullet
        if (useDoubleBulletPrefab && doubleBulletPrefab != null)
        {
            DoubleBullet db = Instantiate(doubleBulletPrefab, spawnPos, spawnRot);
            db.leftBarrel = leftBarrel;
            db.rightBarrel = rightBarrel;
            db.damage = dmg;
            db.Fire(dir);
            return;
        }

        // OLD GUN: single bullet
        if (bulletPrefab == null) return;

        Bullet b = Instantiate(bulletPrefab, spawnPos, spawnRot);
        b.damage = dmg;
        b.Fire(dir);
    }

    void UpdateAmmoUI()
    {
        if (ammoText != null)
            ammoText.text = $"{currentAmmo} / {reserveAmmo}";
    }

    // Pickup vs: aktif silahın reserve'ına ekler
    public void AddReserveAmmo(int amount)
    {
        amount = Mathf.Max(0, amount);
        reserveAmmo += amount;
        SyncAmmoToPool();
        UpdateAmmoUI();
    }

    void SetReloadCircleVisible(bool v)
    {
        if (reloadCircle == null) return;
        reloadCircle.gameObject.SetActive(v);
        if (v) reloadCircle.fillAmount = 0f;
    }

    // Upgrade manager’dan çağırmak istersen
    public void SetNewGunEnabled(bool enabled)
    {
        useDoubleBulletPrefab = enabled;
        ApplyWeaponMode(force: true);
    }
    public void ApplyDamageMultiplier(float mul)
    {
        mul = Mathf.Max(0.1f, mul);
        oldGunDamage *= mul;
        newGunDamage *= mul;
    }

}
