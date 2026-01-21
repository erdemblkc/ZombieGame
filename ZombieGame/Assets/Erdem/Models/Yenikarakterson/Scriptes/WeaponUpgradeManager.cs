using UnityEngine;
using TMPro;

public class WeaponUpgradeManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject missionTextObject;
    public GameObject unlockPopupObject;

    [Header("Weapons")]
    public GameObject gunOld;
    public GameObject gunNew;

    [Header("Upgrade Settings")]
    public int requiredParts = 2;

    private int collectedParts = 0;
    private bool upgradeCompleted = false;

    // 🔹 Wave 1 başladığında çağıracağız
    public void StartWeaponMission()
    {
        if (missionTextObject != null)
            missionTextObject.SetActive(true);
    }

    // 🔹 Silah parçası toplandığında çağıracağız
    public void CollectWeaponPart()
    {
        if (upgradeCompleted) return;

        collectedParts++;

        if (collectedParts >= requiredParts)
        {
            CompleteUpgrade();
        }
    }

    void CompleteUpgrade()
    {
        upgradeCompleted = true;

        if (missionTextObject != null)
            missionTextObject.SetActive(false);

        if (unlockPopupObject != null)
            unlockPopupObject.SetActive(true);

        if (gunOld != null)
            gunOld.SetActive(false);

        if (gunNew != null)
            gunNew.SetActive(true);

        // Popup 2 saniye sonra kapansın
        Invoke(nameof(HidePopup), 2f);
    }

    void HidePopup()
    {
        if (unlockPopupObject != null)
            unlockPopupObject.SetActive(false);
    }
}
