using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Tek bir upgrade kartını temsil eder.
/// UpgradeSelectionUI tarafından doldurulur; düğmeye basılınca parent UI'a bildirir.
/// </summary>
public class UpgradeCardUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image           _iconImage;
    [SerializeField] private TextMeshProUGUI _nameLabel;
    [SerializeField] private TextMeshProUGUI _descLabel;
    [SerializeField] private Image           _categoryColorBar;
    [SerializeField] private Button          _button;

    // Kategori renkleri (ikon yoksa)
    private static readonly Color ColorGun       = new Color(1f,    0.42f, 0.42f); // #FF6B6B
    private static readonly Color ColorMovement  = new Color(0.42f, 0.71f, 1f);    // #6BB5FF
    private static readonly Color ColorUtility   = new Color(0.75f, 0.42f, 1f);    // #C06BFF
    private static readonly Color ColorEvolution = new Color(1f,    0.84f, 0f);    // #FFD700

    private UpgradeSelectionUI _parent;
    private UpgradeData        _data;

    // ── API ───────────────────────────────────────────────────────────────

    /// <summary>Kartı doldurur ve parent callback'ini kaydeder.</summary>
    public void Setup(UpgradeData data, UpgradeSelectionUI parent)
    {
        _data   = data;
        _parent = parent;

        if (_nameLabel != null) _nameLabel.text = data.UpgradeName;
        if (_descLabel != null) _descLabel.text  = data.Description;

        Color catColor = CategoryColor(data.Category);

        if (_iconImage != null)
        {
            if (data.Icon != null)
            {
                _iconImage.sprite = data.Icon;
                _iconImage.color  = Color.white;
            }
            else
            {
                _iconImage.sprite = null;
                _iconImage.color  = catColor;
            }
        }

        if (_categoryColorBar != null)
            _categoryColorBar.color = catColor;

        if (_button != null)
        {
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(OnClick);
        }
    }

    // ── Internal ──────────────────────────────────────────────────────────

    void OnClick()
    {
        _parent?.OnCardSelected(_data);
    }

    static Color CategoryColor(UpgradeCategory cat)
    {
        return cat switch
        {
            UpgradeCategory.Gun       => ColorGun,
            UpgradeCategory.Movement  => ColorMovement,
            UpgradeCategory.Utility   => ColorUtility,
            UpgradeCategory.Evolution => ColorEvolution,
            _                         => Color.white,
        };
    }
}
