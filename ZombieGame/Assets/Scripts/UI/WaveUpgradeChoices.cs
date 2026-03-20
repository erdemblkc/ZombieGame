using UnityEngine;

public class WaveUpgradeChoices : MonoBehaviour
{
    [Header("Refs")]
    public PlayerDamageReceiver player;
    public InfectionSystem infection;
    public GunShooter gun;

    [Header("Upgrade Values")]
    [Range(1f, 3f)] public float damageMultiplier = 1.2f;      // +%20
    [Range(0.1f, 1f)] public float infectionGainMultiplier = 0.7f; // -%30

    [Header("UI (Optional)")]
    public GameObject waveCompletePanel; // seçince kapatmak istersen
    public bool resumeGame = true;        // Time.timeScale = 1
    public bool relockCursor = true;      // FPS'e dönmek için

    void Awake()
    {
        if (player == null) player = FindFirstObjectByType<PlayerDamageReceiver>();
        if (infection == null) infection = FindFirstObjectByType<InfectionSystem>();
        if (gun == null) gun = FindFirstObjectByType<GunShooter>();
    }

    // BUTTON 1: FULL HEAL
    public void Choose_FullHeal()
    {
        if (player != null) player.FullHeal();
        ClosePanel();
    }

    // BUTTON 2: DAMAGE UP
    public void Choose_DamageUp()
    {
        if (gun != null) gun.ApplyDamageMultiplier(damageMultiplier);
        ClosePanel();
    }

    // BUTTON 3: INFECTION SLOW
    public void Choose_InfectionSlow()
    {
        if (infection != null) infection.gainMultiplier *= infectionGainMultiplier; // 1 -> 0.7
        ClosePanel();
    }

    void ClosePanel()
    {
        if (waveCompletePanel != null)
            waveCompletePanel.SetActive(false);

        if (resumeGame)
            Time.timeScale = 1f;

        if (relockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
