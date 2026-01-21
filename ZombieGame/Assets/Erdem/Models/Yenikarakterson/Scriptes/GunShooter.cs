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

    [Header("Reload UI")]
    public Image reloadCircle;          // Radial image (fill)
    public bool showReloadCircle = true;

    [Header("Animator Params")]
    public string aimingParam = "IsAiming";
    public string shootParam = "Shoot";

    [Header("Reload Idle Override")]
    public string idleStateName = "Stand";   // Animator state adı (Base Layer)
    public float idleForceBlend = 0.05f;    // Crossfade süresi

    [Header("Gun Settings")]
    public float damage = 25f;
    public float fireCooldown = 0.12f;
    public float bulletSpawnOffset = 0.03f;

    [Header("Ammo Settings")]
    public int magazineSize = 16;
    public int reserveAmmo = 32;
    public float reloadTime = 1.0f;         // 1 sn dedin
    public bool autoReloadWhenEmpty = true;

    int currentAmmo;
    float nextFireTime;
    bool isReloading;

    void Awake()
    {
        if (anim == null)
        {
            anim = GetComponentInChildren<Animator>();
            if (anim == null) anim = GetComponent<Animator>();
        }

        if (cameraPivot == null && Camera.main != null)
            cameraPivot = Camera.main.transform;

        currentAmmo = magazineSize;
        UpdateAmmoUI();
        SetReloadCircleVisible(false);
    }

    void Update()
    {
        // (Aim bool’unu PlayerController2 zaten set ediyor; burada set etmene gerek yok)
        // istersen kalsın: bool isAiming = Input.GetMouseButton(1); anim.SetBool(aimingParam,isAiming);

        // 🔒 Reload sırasında: Shoot trigger gelse bile anında boşa düşür + Idle’a zorla
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
            UpdateAmmoUI();

            if (anim != null) anim.SetTrigger(shootParam);
            ShootProjectile();
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

        // UI daireyi aç
        if (showReloadCircle) SetReloadCircleVisible(true);

        float t = 0f;
        while (t < reloadTime)
        {
            t += Time.deltaTime;

            // UI progress (0 -> 1)
            if (reloadCircle != null)
                reloadCircle.fillAmount = Mathf.Clamp01(t / reloadTime);

            // Anim: Idle'a zorla (shoot trigger gelirse bastırmak için)
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

        UpdateAmmoUI();

        // UI daireyi kapat
        SetReloadCircleVisible(false);

        isReloading = false;
    }

    void ShootProjectile()
    {
        // Ekstra güvenlik
        if (isReloading) return;

        if (bulletPrefab == null || muzzle == null || cameraPivot == null) return;

        Vector3 dir = cameraPivot.forward;
        Vector3 spawnPos = muzzle.position + muzzle.forward * bulletSpawnOffset;
        Quaternion spawnRot = Quaternion.LookRotation(dir, Vector3.up);

        Bullet b = Instantiate(bulletPrefab, spawnPos, spawnRot);
        b.damage = damage;
        b.Fire(dir);
    }

    void UpdateAmmoUI()
    {
        Debug.Log($"[GunShooter] UpdateAmmoUI | ammoText={(ammoText ? ammoText.name : "NULL")} | {currentAmmo}/{reserveAmmo} | This:{gameObject.name}");
        if (ammoText != null)
            ammoText.text = $"{currentAmmo} / {reserveAmmo}";

    }
    public void AddReserveAmmo(int amount)
    {
        amount = Mathf.Max(0, amount);
        reserveAmmo += amount;

        Debug.Log($"[GunShooter] Ammo added: +{amount} | Now: {currentAmmo}/{reserveAmmo} | This:{gameObject.name}");

        UpdateAmmoUI();
    }


    void SetReloadCircleVisible(bool v)
    {
        if (reloadCircle == null) return;
        reloadCircle.gameObject.SetActive(v);
        if (v) reloadCircle.fillAmount = 0f;
    }
}
