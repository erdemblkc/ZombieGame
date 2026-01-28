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

    [Header("Gun Shooter Ref")]
    public GunShooter shooter;

    [Header("Text Settings")]
    public string missionTextFormat = "Silah parçaları topla {0}/{1}";

    [Header("Weapons - Assign in Inspector")]
    public GameObject gunOld;
    public GameObject gunNew;

    [Header("Upgrade Settings")]
    public int requiredParts = 2;
    public bool startOnGameStart = true;

    int collectedParts = 0;
    bool upgradeCompleted = false;

    // ZombieSpawner eski çağrısı için
    public void StartWeaponMission() => StartMission();

    void Awake()
    {
        if (missionTextTMP == null && missionTextObject != null)
            missionTextTMP = missionTextObject.GetComponentInChildren<TMP_Text>(true);

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
            missionTextObject.SetActive(visible);
    }

    void CompleteUpgrade()
    {
        upgradeCompleted = true;

        // Mission yazısı kapansın
        if (missionTextObject != null)
            missionTextObject.SetActive(false);

        // Silah değiştir
        if (gunOld != null) gunOld.SetActive(false);
        if (gunNew != null) gunNew.SetActive(true);

        if (shooter != null)
            shooter.useDoubleBulletPrefab = true;

        // ✅ SADECE TOAST GÖSTER (background yok)
        if (toast != null)
            toast.Show(newWeaponSprite, toastTitle);
    }
}
