using UnityEngine;

public class WaveCompleteManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject panel;

    [Header("Spawner")]
    public ZombieSpawner1 spawner;

    void Start()
    {
        if (panel != null) panel.SetActive(false);
    }

    public void ShowPanel()
    {
        if (panel == null)
        {
            Debug.LogError("[WaveCompleteManager] Panel reference is NULL!");
            return;
        }

        Debug.Log("[WaveCompleteManager] ShowPanel()");
        Time.timeScale = 0f;
        panel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Butonlarýn OnClick'inden çaðrýlacak
    public void OnUpgradeSelected()
    {
        Debug.Log("[WaveCompleteManager] OnUpgradeSelected()");

        if (panel != null) panel.SetActive(false);

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (spawner == null)
        {
            Debug.LogError("[WaveCompleteManager] Spawner reference is NULL! Inspector'dan ZombieSpawner1 olan objeyi ata.");
            return;
        }

        spawner.SpawnNextWaveFromUI();
    }
}
