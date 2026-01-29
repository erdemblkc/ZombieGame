using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class GunShooter : MonoBehaviour
{
    [Header("Refs")]
    public Animator anim;
    public Transform cameraPivot;
    public TextMeshProUGUI ammoText;

    [Header("Audio Source")]
    public AudioSource audioSource; // Player üzerindeki AudioSource

    [Header("Guns (Hierarchy Children)")]
    public GameObject gunOld;
    public GameObject gunNew;
    public WeaponStats currentWeapon;

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

    // --- SİLAH AÇMA FONKSİYONU (Hafızalı) ---
    public void SetNewGunEnabled(bool enabled)
    {
        // Hafızaya kaydet
        if (enabled) GlobalGameState.IsWeaponUpgraded = true;

        if (enabled) UpgradeToNewGun();
        else SwitchToOldGun();
    }

    public void AddReserveAmmo(int amount) { amount = Mathf.Max(0, amount); reserveAmmo += amount; UpdateAmmoUI(); }

    public void ApplyDamageMultiplier(float mul)
    {
        mul = Mathf.Max(0.1f, mul);
        if (currentWeapon != null) currentWeapon.damage *= mul;
        if (gunOld != null) { var ws = gunOld.GetComponent<WeaponStats>(); if (ws != null) ws.damage *= mul; }
        if (gunNew != null) { var ws = gunNew.GetComponent<WeaponStats>(); if (ws != null) ws.damage *= mul; }
    }

    void Awake()
    {
        if (anim == null) { anim = GetComponentInChildren<Animator>(); if (anim == null) anim = GetComponent<Animator>(); }
        if (cameraPivot == null && Camera.main != null) cameraPivot = Camera.main.transform;

        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = GetComponentInParent<AudioSource>();

        if (currentWeapon == null)
        {
            if (gunNew != null && gunNew.activeInHierarchy) SetWeapon(gunNew);
            else if (gunOld != null) SetWeapon(gunOld);
        }

        SetReloadCircleVisible(false);
        UpdateAmmoUI();
        if (anim != null) anim.SetBool(aimingParam, false);

        // --- HAFIZA KONTROLÜ 1 (Awake) ---
        if (GlobalGameState.IsWeaponUpgraded)
        {
            UpgradeToNewGun();
        }
    }

    // --- YENİ EKLENEN KISIM (Start) ---
    // Bu kısım, Awake'ten sonra devreye girer ve animasyonların silahı bozmasını engeller.
    void Start()
    {
        if (GlobalGameState.IsWeaponUpgraded)
        {
            UpgradeToNewGun();
        }
    }

    void Update()
    {
        if (currentWeapon == null) return;

        if (anim != null) anim.SetBool(aimingParam, Input.GetMouseButton(1));

        if (isReloading) { if (anim != null) anim.ResetTrigger(shootParam); return; }

        if (Input.GetKeyDown(KeyCode.R)) { TryReload(); return; }
        if (currentAmmo <= 0) { if (currentWeapon.autoReloadWhenEmpty) TryReload(); return; }

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

        // --- SES: RELOAD ---
        if (audioSource != null && currentWeapon.reloadSound != null)
        {
            audioSource.pitch = 1.0f;
            audioSource.PlayOneShot(currentWeapon.reloadSound);
        }

        float t = 0f;
        while (t < reloadTime)
        {
            t += Time.deltaTime;
            if (reloadCircle != null) reloadCircle.fillAmount = Mathf.Clamp01(t / reloadTime);
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
        if (isReloading || cameraPivot == null || currentWeapon == null) return;

        // --- SES: ATEŞ ---
        if (audioSource != null && currentWeapon.fireSound != null)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(currentWeapon.fireSound, currentWeapon.fireVolume);
        }

        Transform firePoint = currentWeapon.muzzle;
        if (firePoint == null) return;

        Vector3 dir = cameraPivot.forward;
        Vector3 spawnPos = firePoint.position;
        Quaternion spawnRot = Quaternion.LookRotation(dir, Vector3.up);

        if (currentWeapon.useDoubleBullet && currentWeapon.doubleBulletPrefab != null)
        {
            DoubleBullet db = Instantiate(currentWeapon.doubleBulletPrefab, spawnPos, spawnRot);
            db.leftBarrel = currentWeapon.leftBarrel;
            db.rightBarrel = currentWeapon.rightBarrel;
            db.damage = currentWeapon.damage;
            db.Fire(dir);
        }
        else if (currentWeapon.singleBulletPrefab != null)
        {
            Bullet b = Instantiate(currentWeapon.singleBulletPrefab, spawnPos, spawnRot);
            b.damage = currentWeapon.damage;
            b.Fire(dir);
        }
    }

    void UpdateAmmoUI() { if (ammoText != null) ammoText.text = $"{currentAmmo} / {reserveAmmo}"; }
    void SetReloadCircleVisible(bool v) { if (reloadCircle != null) { reloadCircle.gameObject.SetActive(v); if (v) reloadCircle.fillAmount = 0f; } }

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
        if (newStats == null) return;

        currentWeapon = newStats;
        currentAmmo = currentWeapon.magazineSize;
        reserveAmmo = currentWeapon.reserveStart;
        isReloading = false;
        nextFireTime = 0f;

        UpdateAmmoUI();
        SetReloadCircleVisible(false);
    }
}