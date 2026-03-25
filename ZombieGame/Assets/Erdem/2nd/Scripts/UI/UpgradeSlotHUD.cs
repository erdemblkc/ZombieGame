using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Alt ekranda 4 upgrade slotunu gösterir.
/// UpgradeSlotManager.OnSlotsChanged eventini dinler ve otomatik güncellenir.
///
/// Sahne kurulumu:
///   1. Canvas altında 4 SlotPanel oluştur.
///   2. Her SlotPanel'e bir slot entry ata (Inspector array).
///   3. Bu scripti sahnedeki herhangi bir UI GameObject'ine ekle.
/// </summary>
public class UpgradeSlotHUD : MonoBehaviour
{
    [System.Serializable]
    public struct SlotEntry
    {
        [Tooltip("Kart GameObject'i (Image + label içerir).")]
        public GameObject  root;
        [Tooltip("Upgrade ikonunu gösterecek Image.")]
        public Image       iconImage;
        [Tooltip("Upgrade ismini gösterecek label.")]
        public TextMeshProUGUI nameLabel;
        [Tooltip("Slot boşken gösterilecek overlay (opsiyonel).")]
        public GameObject  emptyOverlay;
    }

    [Header("Slot Entries")]
    [SerializeField] private SlotEntry[] _slots = new SlotEntry[4];

    // Kategori renkleri (ikon yoksa)
    private static readonly Color ColorGun       = new Color(1f,    0.42f, 0.42f);
    private static readonly Color ColorMovement  = new Color(0.42f, 0.71f, 1f);
    private static readonly Color ColorUtility   = new Color(0.75f, 0.42f, 1f);
    private static readonly Color ColorEvolution = new Color(1f,    0.84f, 0f);
    private static readonly Color ColorEmpty     = new Color(0.3f,  0.3f,  0.3f, 0.5f);

    // ── Lifecycle ──────────────────────────────────────────────────────────

    void Start()
    {
        if (UpgradeSlotManager.Instance != null)
            UpgradeSlotManager.Instance.OnSlotsChanged += Refresh;

        Refresh();
    }

    void OnDestroy()
    {
        if (UpgradeSlotManager.Instance != null)
            UpgradeSlotManager.Instance.OnSlotsChanged -= Refresh;
    }

    // ── Internal ──────────────────────────────────────────────────────────

    void Refresh()
    {
        var manager = UpgradeSlotManager.Instance;

        for (int i = 0; i < _slots.Length; i++)
        {
            UpgradeData data = manager != null ? manager.GetSlot(i) : null;
            SetSlot(i, data);
        }
    }

    void SetSlot(int index, UpgradeData data)
    {
        if (index >= _slots.Length) return;
        SlotEntry entry = _slots[index];

        bool occupied = data != null;

        if (entry.emptyOverlay != null)
            entry.emptyOverlay.SetActive(!occupied);

        if (occupied)
        {
            Color catColor = CategoryColor(data.Category);

            if (entry.nameLabel != null) entry.nameLabel.text = data.UpgradeName;

            if (entry.iconImage != null)
            {
                if (data.Icon != null)
                {
                    entry.iconImage.sprite = data.Icon;
                    entry.iconImage.color  = Color.white;
                }
                else
                {
                    entry.iconImage.sprite = null;
                    entry.iconImage.color  = catColor;
                }
            }
        }
        else
        {
            if (entry.nameLabel  != null) entry.nameLabel.text   = "— SLOT —";
            if (entry.iconImage  != null) { entry.iconImage.sprite = null; entry.iconImage.color = ColorEmpty; }
        }
    }

    static Color CategoryColor(UpgradeCategory cat) => cat switch
    {
        UpgradeCategory.Gun       => ColorGun,
        UpgradeCategory.Movement  => ColorMovement,
        UpgradeCategory.Utility   => ColorUtility,
        UpgradeCategory.Evolution => ColorEvolution,
        _                         => Color.white,
    };
}
