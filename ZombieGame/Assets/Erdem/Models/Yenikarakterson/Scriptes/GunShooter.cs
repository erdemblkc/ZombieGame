using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class GunShooter : MonoBehaviour
{
    [Header("Refs")]
    public Animator anim;
    public Transform cameraPivot;
    // public Transform muzzle; // KALDIRILDI: Artık silahtan alınıyor.
    public TextMeshProUGUI ammoText;

    [Header("Guns (Hierarchy Children)")]
    public GameObject gunOld;   // Gun_Old
    public GameObject gunNew;   // Gun_New
    public WeaponStats currentWeapon; // aktif silahın stats'ı

    [Header("Reload UI")]
    public Image reloadCircle;
    public bool showReloadCircle = true;

    [Header("Animator Params")]
    public string aimingParam = "IsAiming";
    public string shootParam = "Shoot";

    // ---- runtime ----
    int currentAmmo;
    int reserveAmmo;

    float nextFireTime;
    bool isReloading;

    // =========================
    // API (Dışarıdan çağrılanlar)
    // =========================

    public void SetNewGunEnabled(bool enabled)
    {
        if (enabled) UpgradeToNewGun();
        else SwitchToOldGun();
    }

    public void AddReserveAmmo(int amount)
    {
        amount = Mathf.Max(0, amount);
        reserveAmmo += amount;
        UpdateAmmoUI();
    }

    public void ApplyDamageMultiplier(float mul)
    {
        mul = Mathf.Max(0.1f, mul);

        if (currentWeapon != null) currentWeapon.damage *= mul;

        // Pasif silahların da hasarını güncelle
        if (gunOld != null)
        {
            var ws = gunOld.GetComponent<WeaponStats>();
            if (ws != null) ws.damage *= mul;
        }
        if (gunNew != null)
        {
            var ws = gunNew.GetComponent<WeaponStats>();
            if (ws != null) ws.damage *= mul;
        }
    }

    // =========================
    // UNITY
    // =========================

    void Awake()
    {
        if (anim == null)
        {
            anim = GetComponentInChildren<Animator>();
            if (anim == null) anim = GetComponent<Animator>();
        }

        if (cameraPivot == null && Camera.main != null)
            cameraPivot = Camera.main.transform;

        // Başlangıçta aktif silah seçimi
        if (currentWeapon == null)
        {
            if (gunNew != null && gunNew.activeInHierarchy)
                SetWeapon(gunNew);
            else if (gunOld != null)
                SetWeapon(gunOld);
        }

        SetReloadCircleVisible(false);
        UpdateAmmoUI();

        if (anim != null) anim.SetBool(aimingParam, false);
    }

    void Update()
    {
        if (currentWeapon == null) return;

        // AIM
        if (anim != null)
            anim.SetBool(aimingParam, Input.GetMouseButton(1));

        if (isReloading)
        {
            if (anim != null) anim.ResetTrigger(shootParam);
            return;
        }

        // Reload Input
        if (Input.GetKeyDown(KeyCode.R))
        {
            TryReload();
            return;
        }

        // Auto Reload
        if (currentAmmo <= 0)
        {
            if (currentWeapon.autoReloadWhenEmpty) TryReload();
            return;
        }

        // Shoot Input
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            if (currentAmmo <= 0) return;

            nextFireTime = Time.time + currentWeapon.fireCooldown;
            currentAmmo--;
            UpdateAmmoUI();

            if (anim != null) anim.SetTrigger(shootParam);
            ShootProjectile();
        }
    }

    void TryReload()
    {
        if (isReloading) return;
        if (currentWeapon == null) return;
        if (currentAmmo >= currentWeapon.magazineSize) return;
        if (reserveAmmo <= 0) return;

        StartCoroutine(ReloadRoutine());
    }

    IEnumerator ReloadRoutine()
    {
        isReloading = true;
        float reloadTime = currentWeapon.reloadTime;
        nextFireTime = Mathf.Max(nextFireTime, Time.time + reloadTime);

        if (showReloadCircle) SetReloadCircleVisible(true);

        float t = 0f;
        while (t < reloadTime)
        {
            t += Time.deltaTime;
            if (reloadCircle != null)
                reloadCircle.fillAmount = Mathf.Clamp01(t / reloadTime);

            if (anim != null) anim.ResetTrigger(shootParam);
            yield return null;
        }

        int need = currentWeapon.magazineSize - currentAmmo;
        int take = Mathf.Min(need, reserveAmmo);
        currentAmmo += take;
        reserveAmmo -= take;

        UpdateAmmoUI();
        SetReloadCircleVisible(false);
        isReloading = false;
    }

    void ShootProjectile()
    {
        if (isReloading) return;
        if (cameraPivot == null || currentWeapon == null) return;

        // YENİ: Muzzle'ı aktif silahtan al
        Transform firePoint = currentWeapon.muzzle;
        if (firePoint == null)
        {
            Debug.LogError($"HATA: {currentWeapon.name} objesindeki WeaponStats scriptinde 'Muzzle' boş! Lütfen atama yap.");
            return;
        }

        Vector3 dir = cameraPivot.forward;
        Vector3 spawnPos = firePoint.position;
        Quaternion spawnRot = Quaternion.LookRotation(dir, Vector3.up);

        // Debug için konsola yazalım (Sorun çözülünce silebilirsin)
        // Debug.Log($"Ateşlenen: {currentWeapon.name} | Hasar: {currentWeapon.damage} | DoubleMode: {currentWeapon.useDoubleBullet}");

        // --- DOUBLE BULLET ---
        if (currentWeapon.useDoubleBullet)
        {
            if (currentWeapon.doubleBulletPrefab != null)
            {
                DoubleBullet db = Instantiate(currentWeapon.doubleBulletPrefab, spawnPos, spawnRot);
                // Namlu çıkışlarını aktar
                db.leftBarrel = currentWeapon.leftBarrel;
                db.rightBarrel = currentWeapon.rightBarrel;
                // Hasarı aktar
                db.damage = currentWeapon.damage;
                db.Fire(dir);
            }
            return;
        }

        // --- SINGLE BULLET ---
        if (currentWeapon.singleBulletPrefab != null)
        {
            Bullet b = Instantiate(currentWeapon.singleBulletPrefab, spawnPos, spawnRot);
            b.damage = currentWeapon.damage;
            b.Fire(dir);
        }
    }

    void UpdateAmmoUI()
    {
        if (ammoText != null)
            ammoText.text = $"{currentAmmo} / {reserveAmmo}";
    }

    void SetReloadCircleVisible(bool v)
    {
        if (reloadCircle != null)
        {
            reloadCircle.gameObject.SetActive(v);
            if (v) reloadCircle.fillAmount = 0f;
        }
    }

    public void UpgradeToNewGun()
    {
        if (gunOld != null) gunOld.SetActive(false);
        if (gunNew != null) gunNew.SetActive(true);

        if (gunNew != null) SetWeapon(gunNew);
    }

    public void SwitchToOldGun()
    {
        if (gunNew != null) gunNew.SetActive(false);
        if (gunOld != null) gunOld.SetActive(true);

        if (gunOld != null) SetWeapon(gunOld);
    }

    void SetWeapon(GameObject gunObj)
    {
        if (gunObj == null) return;
        WeaponStats newStats = gunObj.GetComponent<WeaponStats>();

        if (newStats == null)
        {
            Debug.LogError($"WeaponStats eksik: {gunObj.name}");
            return;
        }

        currentWeapon = newStats;

        // Mermi bilgilerini yenile
        currentAmmo = currentWeapon.magazineSize;
        reserveAmmo = currentWeapon.reserveStart;
        isReloading = false;
        nextFireTime = 0f;

        UpdateAmmoUI();
        SetReloadCircleVisible(false);
    }
}