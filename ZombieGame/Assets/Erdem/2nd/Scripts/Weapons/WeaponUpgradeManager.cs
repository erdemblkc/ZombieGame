using UnityEngine;
using TMPro;

public class WeaponUpgradeManager : MonoBehaviour
{
    [Header("UI - Assign in Inspector")]
    public GameObject missionTextObject;
    public TMP_Text missionTextTMP;

    [Header("Toast (NEW WEAPON)")]
    public WeaponUnlockToast toast;          // sahnedeki WeaponUnlockToast objesini buraya ata
    public Sprite newWeaponSprite;           // yeni silah png/sprite (UI sprite)
    public string toastTitle = "NEW WEAPON";

    [Header("Gun Shooter Ref (ÖNEMLİ)")]
    public GunShooter shooter; // GunShooter scriptini buraya sürükle

    [Header("Text Settings")]
    [Tooltip("Upgrade yazısını ekranda göster. Kapat = sağ üstteki yazı gizlenir.")]
    public bool showMissionText = false;
    public string missionTextFormat = "Upgrade your weapon! Collect parts {0}/{1}";

    [Header("Weapons - Assign in Inspector")]
    public GameObject gunOld;
    public GameObject gunNew;

    [Header("Upgrade Settings")]
    public int requiredParts = 2;
    public bool startOnGameStart = true;

    int collectedParts = 0;
    bool upgradeCompleted = false;

    // ZombieSpawner eski çağrısı için wrapper
    public void StartWeaponMission() => StartMission();

    void Awake()
    {
        if (missionTextTMP == null && missionTextObject != null)
            missionTextTMP = missionTextObject.GetComponentInChildren<TMP_Text>(true);

        // Başlangıçta eski silah açık, yeni silah kapalı olsun
        if (gunNew != null) gunNew.SetActive(false);
        if (gunOld != null) gunOld.SetActive(true);
    }

    void Start()
    {
        if (startOnGameStart)
            StartMission();
    }

    public void StartMission()
    {
        if (upgradeCompleted) return;

        collectedParts = 0;
        SetMissionVisible(true);
        UpdateMissionText();
    }

    public void CollectWeaponPart()
    {
        if (upgradeCompleted) return;

        collectedParts++;
        UpdateMissionText();

        if (collectedParts >= requiredParts)
            CompleteUpgrade();
    }

    void UpdateMissionText()
    {
        if (missionTextTMP != null)
            missionTextTMP.text = string.Format(missionTextFormat, collectedParts, requiredParts);
    }

    void SetMissionVisible(bool visible)
    {
        if (missionTextObject != null)
            missionTextObject.SetActive(visible && showMissionText);
    }

    void CompleteUpgrade()
    {
        upgradeCompleted = true;

        // Mission yazısı kapansın
        if (missionTextObject != null)
            missionTextObject.SetActive(false);

        // --- DÜZELTME BURADA ---
        // Objeleri manuel açıp kapatmak yerine GunShooter'a haber veriyoruz.
        // Bu sayede GunShooter hem objeyi değiştiriyor hem de DAMAGE/MERMİ bilgisini güncelliyor.
        if (shooter != null)
        {
            shooter.SetNewGunEnabled(true);
        }
        else
        {
            Debug.LogError("WeaponUpgradeManager: 'Shooter' referansı boş! Inspector'dan ata.");
            // Acil durum yedeği (shooter yoksa manuel açalım en azından görüntü değişsin)
            if (gunOld != null) gunOld.SetActive(false);
            if (gunNew != null) gunNew.SetActive(true);
        }

        // Toast göster
        if (toast != null)
            toast.Show(newWeaponSprite, toastTitle);
    }
}