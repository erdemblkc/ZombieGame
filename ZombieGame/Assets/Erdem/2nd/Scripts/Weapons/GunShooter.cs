using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Handles weapon firing, magazine reload, weapon switching, and UI updates.
/// Ammo reserve is unlimited — only the magazine (clip) size limits burst firing.
/// Integrates with GunModifierStack for damage/fire-rate/spread-shot upgrades.
/// </summary>
public class GunShooter : MonoBehaviour
{
    [Header("Refs")]
    public Animator anim;
    public Transform cameraPivot;
    public TextMeshProUGUI ammoText;

    [Header("Audio Source")]
    public AudioSource audioSource;

    [Header("Guns (Hierarchy Children)")]
    public GameObject gunOld;
    public GameObject gunNew;
    public WeaponStats currentWeapon;

    [Header("Reload UI")]
    public Image reloadCircle;
    public bool showReloadCircle = true;

    [Header("Animator Params")]
    public string aimingParam = "IsAiming";
    public string shootParam  = "Shoot";

    // ---- runtime ----
    int   currentAmmo;
    float nextFireTime;
    bool  isReloading;

    // ---- upgrade modifier stack ----
    GunModifierStack _modStack;

    // ── Lifecycle ──────────────────────────────────────────────────────────

    void Awake()
    {
        _modStack = GetComponent<GunModifierStack>();

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

        if (GlobalGameState.IsWeaponUpgraded) UpgradeToNewGun();
    }

    void Start()
    {
        if (GlobalGameState.IsWeaponUpgraded) UpgradeToNewGun();
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
            float cooldownMul = _modStack != null ? _modStack.FireCooldownMultiplier : 1f;
            nextFireTime = Time.time + currentWeapon.fireCooldown * cooldownMul;
            currentAmmo--;
            UpdateAmmoUI();

            if (anim != null) anim.SetTrigger(shootParam);
            ShootProjectile();
        }
    }

    // ── Reload ─────────────────────────────────────────────────────────────

    void TryReload()
    {
        if (isReloading) return;
        if (currentWeapon == null) return;
        if (currentAmmo >= currentWeapon.magazineSize) return;
        // No reserve check — ammo is unlimited
        StartCoroutine(ReloadRoutine());
    }

    IEnumerator ReloadRoutine()
    {
        isReloading = true;
        float reloadTime = currentWeapon.reloadTime;
        nextFireTime = Mathf.Max(nextFireTime, Time.time + reloadTime);

        if (showReloadCircle) SetReloadCircleVisible(true);

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

        // Unlimited reserve: always fill the magazine completely
        currentAmmo = currentWeapon.magazineSize;

        UpdateAmmoUI();
        SetReloadCircleVisible(false);
        isReloading = false;
    }

    // ── Shooting ───────────────────────────────────────────────────────────

    void ShootProjectile()
    {
        if (isReloading || cameraPivot == null || currentWeapon == null) return;

        if (audioSource != null && currentWeapon.fireSound != null)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(currentWeapon.fireSound, currentWeapon.fireVolume);
        }

        Transform firePoint = currentWeapon.muzzle;
        if (firePoint == null) return;

        Vector3 dir     = cameraPivot.forward;
        Vector3 spawnPos = firePoint.position;

        float infectionDmgMul = InfectionSystem.Instance != null ? InfectionSystem.Instance.DamageBonusMultiplier : 1f;
        float effectiveDamage = currentWeapon.damage * (_modStack != null ? _modStack.DamageMultiplier : 1f) * infectionDmgMul;
        int   piercing        = _modStack != null ? _modStack.PiercingCount  : 0;
        int   ricochet        = _modStack != null ? _modStack.RicochetCount  : 0;

        SpawnBullet(spawnPos, dir, effectiveDamage, piercing, ricochet);

        // Spread Shot: fire 2 extra bullets in a cone
        if (_modStack != null && _modStack.HasSpreadShot)
        {
            SpawnBullet(spawnPos, SpreadDir(dir, cameraPivot,  15f, 0f),  effectiveDamage * 0.7f, piercing, ricochet);
            SpawnBullet(spawnPos, SpreadDir(dir, cameraPivot, -15f, 0f),  effectiveDamage * 0.7f, piercing, ricochet);
        }
    }

    /// <summary>Spawns a single bullet (or double-bullet) in the given direction.</summary>
    void SpawnBullet(Vector3 pos, Vector3 dir, float damage, int piercing, int ricochet = 0)
    {
        Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);

        if (currentWeapon.useDoubleBullet && currentWeapon.doubleBulletPrefab != null)
        {
            DoubleBullet db = Instantiate(currentWeapon.doubleBulletPrefab, pos, rot);
            db.leftBarrel   = currentWeapon.leftBarrel;
            db.rightBarrel  = currentWeapon.rightBarrel;
            db.damage       = damage;
            db.piercingCount = piercing;
            db.ricochetCount = ricochet;
            db.Fire(dir);
        }
        else if (currentWeapon.singleBulletPrefab != null)
        {
            Bullet b = Instantiate(currentWeapon.singleBulletPrefab, pos, rot);
            b.damage        = damage;
            b.piercingCount = piercing;
            b.ricochetCount = ricochet;
            b.Fire(dir);
        }
    }

    /// <summary>Returns a direction rotated by yaw/pitch relative to the camera.</summary>
    static Vector3 SpreadDir(Vector3 baseDir, Transform cam, float yawDeg, float pitchDeg)
    {
        Quaternion rot = Quaternion.AngleAxis(yawDeg, cam.up) * Quaternion.AngleAxis(pitchDeg, cam.right);
        return rot * baseDir;
    }

    // ── UI ─────────────────────────────────────────────────────────────────

    void UpdateAmmoUI()
    {
        if (ammoText != null && currentWeapon != null)
            ammoText.text = $"{currentAmmo} / {currentWeapon.magazineSize}  \u221E";
    }

    void SetReloadCircleVisible(bool v)
    {
        if (reloadCircle != null) { reloadCircle.gameObject.SetActive(v); if (v) reloadCircle.fillAmount = 0f; }
    }

    // ── Public API ─────────────────────────────────────────────────────────

    /// <summary>Switches to the upgraded gun and saves state.</summary>
    public void SetNewGunEnabled(bool enabled)
    {
        if (enabled) GlobalGameState.IsWeaponUpgraded = true;
        if (enabled) UpgradeToNewGun();
        else         SwitchToOldGun();
    }

    /// <summary>No-op: ammo is now unlimited. Kept for API compatibility.</summary>
    public void AddReserveAmmo(int amount) { /* unlimited — no action needed */ }

    public void ApplyDamageMultiplier(float mul)
    {
        mul = Mathf.Max(0.1f, mul);
        if (currentWeapon != null) currentWeapon.damage *= mul;
        if (gunOld != null) { var ws = gunOld.GetComponent<WeaponStats>(); if (ws != null) ws.damage *= mul; }
        if (gunNew != null) { var ws = gunNew.GetComponent<WeaponStats>(); if (ws != null) ws.damage *= mul; }
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
        if (newStats == null) return;

        currentWeapon = newStats;
        currentAmmo   = currentWeapon.magazineSize;
        isReloading   = false;
        nextFireTime  = 0f;

        UpdateAmmoUI();
        SetReloadCircleVisible(false);
    }
}
