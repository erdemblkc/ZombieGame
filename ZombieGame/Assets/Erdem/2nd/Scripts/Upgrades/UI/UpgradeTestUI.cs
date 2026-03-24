using UnityEngine;

/// <summary>
/// Debug UI for testing the upgrade slot system in-editor.
/// Uses OnGUI (no Canvas setup required). Attach to any active GameObject in the GameScene.
/// Assign UpgradeData assets via the Inspector slots, then press the buttons at runtime.
/// </summary>
public class UpgradeTestUI : MonoBehaviour
{
    [Header("Player Reference")]
    [SerializeField] private UpgradeSlotManager _slotManager;

    [Header("Available Test Upgrades (assign UpgradeData .asset files)")]
    [SerializeField] private UpgradeData[] _testUpgrades = new UpgradeData[8];

    private const float PanelX      = 10f;
    private const float PanelY      = 10f;
    private const float ButtonWidth  = 200f;
    private const float ButtonHeight = 28f;
    private const float Spacing      = 4f;

    void Awake()
    {
        if (_slotManager == null)
            _slotManager = FindFirstObjectByType<UpgradeSlotManager>();
    }

    void OnGUI()
    {
        if (_slotManager == null)
        {
            GUI.Label(new Rect(PanelX, PanelY, 300, 20), "[UpgradeTestUI] No UpgradeSlotManager found!");
            return;
        }

        float y = PanelY;

        GUI.Box(new Rect(PanelX - 5, y - 5, ButtonWidth + 10, 470), "Upgrade Test Panel");
        y += 20f;

        // ── Current slots ─────────────────────────────────────────────────
        GUI.Label(new Rect(PanelX, y, ButtonWidth, ButtonHeight), "── Active Slots ──");
        y += ButtonHeight;

        for (int i = 0; i < UpgradeSlotManager.SlotCount; i++)
        {
            var data = _slotManager.GetSlot(i);
            string slotLabel = data != null ? $"[{i}] {data.UpgradeName}" : $"[{i}] Empty";
            GUI.Label(new Rect(PanelX, y, ButtonWidth - 70, ButtonHeight), slotLabel);

            if (data != null)
            {
                if (GUI.Button(new Rect(PanelX + ButtonWidth - 65, y, 60f, ButtonHeight - 2), "Remove"))
                    _slotManager.RemoveUpgrade(i);
            }
            y += ButtonHeight + Spacing;
        }

        y += 8f;
        GUI.Label(new Rect(PanelX, y, ButtonWidth, ButtonHeight), "── Add Upgrades ──");
        y += ButtonHeight;

        // ── Add upgrade buttons ────────────────────────────────────────────
        for (int i = 0; i < _testUpgrades.Length; i++)
        {
            var upg = _testUpgrades[i];
            if (upg == null) continue;

            bool alreadyInstalled = _slotManager.HasUpgrade(upg);
            GUI.enabled = !alreadyInstalled;

            if (GUI.Button(new Rect(PanelX, y, ButtonWidth, ButtonHeight - 2), upg.UpgradeName))
            {
                // Find first empty slot
                bool added = false;
                for (int s = 0; s < UpgradeSlotManager.SlotCount; s++)
                {
                    if (_slotManager.GetSlot(s) == null)
                    {
                        _slotManager.AddUpgrade(s, upg);
                        added = true;
                        break;
                    }
                }
                if (!added)
                    Debug.LogWarning("[UpgradeTestUI] All slots are full.");
            }

            GUI.enabled = true;
            y += ButtonHeight + Spacing;
        }

        y += 8f;
        if (GUI.Button(new Rect(PanelX, y, ButtonWidth, ButtonHeight), "Clear All Slots"))
            _slotManager.ClearAll();
    }
}
