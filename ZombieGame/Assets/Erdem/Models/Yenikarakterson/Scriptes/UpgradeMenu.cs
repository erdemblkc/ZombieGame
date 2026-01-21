using UnityEngine;
using UnityEngine.UI;

public class UpgradeMenu : MonoBehaviour
{
    [Header("UI Refs")]
    public GameObject upgradePanel;   // UpgradePanel (SetActive false)
    public Button speedButton;
    public Button ammoButton;
    public Button staminaButton;

    [Header("Debug (optional)")]
    public KeyCode toggleKey = KeyCode.F3;
    public bool allowDebugToggle = true;

    [Header("Upgrade Values")]
    [Tooltip("Normal yürüyüþ hýzýna çarpan (moveSpeed)")]
    public float moveSpeedMultiplier = 1.15f;   // +%15 walk
    [Tooltip("Sprint hýzýna çarpan (sprintSpeed)")]
    public float sprintSpeedMultiplier = 1.10f; // +%10 sprint

    [Tooltip("Reserve ammo eklenecek miktar")]
    public int ammoBonus = 30;

    [Tooltip("Enerji kapasitesi çarpaný (maxEnergy)")]
    public float maxEnergyMultiplier = 1.20f;   // +%20 max energy
    [Tooltip("Enerji refill süresi çarpaný (energyRefillTime) - küçülürse daha hýzlý dolar")]
    public float refillTimeMultiplier = 0.85f;  // %15 daha hýzlý dolsun

    private PlayerController2 player;
    private GunShooter gun;

    private bool isOpen;

    void Awake()
    {
        if (speedButton != null) speedButton.onClick.AddListener(ChooseSpeed);
        if (ammoButton != null) ammoButton.onClick.AddListener(ChooseAmmo);
        if (staminaButton != null) staminaButton.onClick.AddListener(ChooseStamina);

        // güvenli baþlangýç
        CloseMenuImmediate();
    }

    void Update()
    {
        if (!allowDebugToggle) return;

        if (Input.GetKeyDown(toggleKey))
        {
            if (isOpen) CloseMenu();
            else OpenMenu();
        }
    }

    /// <summary>
    /// Wave bitince çaðýracaðýz.
    /// player ve gun referanslarýný verirsen upgrade direkt doðru objeye iþler.
    /// </summary>
    public void ShowAfterWave(PlayerController2 playerRef, GunShooter gunRef)
    {
        player = playerRef;
        gun = gunRef;
        OpenMenu();
    }

    // -------------------- UI OPEN/CLOSE --------------------

    private void OpenMenu()
    {
        isOpen = true;

        if (upgradePanel != null)
            upgradePanel.SetActive(true);

        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void CloseMenu()
    {
        isOpen = false;

        if (upgradePanel != null)
            upgradePanel.SetActive(false);

        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void CloseMenuImmediate()
    {
        isOpen = false;

        if (upgradePanel != null)
            upgradePanel.SetActive(false);

        // Awake anýnda timescale zaten 1 ama garanti olsun
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // -------------------- BUTTON ACTIONS --------------------

    private void ChooseSpeed()
    {
        if (player != null)
        {
            player.moveSpeed *= moveSpeedMultiplier;
            player.sprintSpeed *= sprintSpeedMultiplier;

            Debug.Log($"[UpgradeMenu] Speed chosen. moveSpeed={player.moveSpeed}, sprintSpeed={player.sprintSpeed}");
        }
        else
        {
            Debug.LogWarning("[UpgradeMenu] Player reference is null. Speed upgrade not applied.");
        }

        CloseMenu();
    }

    private void ChooseAmmo()
    {
        if (gun != null)
        {
            gun.AddReserveAmmo(ammoBonus);
            Debug.Log($"[UpgradeMenu] Ammo chosen. +{ammoBonus} reserve ammo");
        }
        else
        {
            Debug.LogWarning("[UpgradeMenu] Gun reference is null. Ammo upgrade not applied.");
        }

        CloseMenu();
    }

    private void ChooseStamina()
    {
        if (player != null)
        {
            // maxEnergy artýþý
            player.maxEnergy *= maxEnergyMultiplier;

            // refill daha hýzlý (time küçülür)
            player.energyRefillTime *= refillTimeMultiplier;

            // currentEnergy script içinde private, ama maxEnergy artýnca UI "MaxEnergy" artmýþ olur.
            // currentEnergy hemen full olmasýn istiyorsan dokunmuyoruz.

            Debug.Log($"[UpgradeMenu] Stamina chosen. maxEnergy={player.maxEnergy}, refillTime={player.energyRefillTime}");
        }
        else
        {
            Debug.LogWarning("[UpgradeMenu] Player reference is null. Stamina upgrade not applied.");
        }

        CloseMenu();
    }
}
