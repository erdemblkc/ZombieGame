using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Tek bir upgrade kartını temsil eder.
/// UpgradeSelectionUI tarafından doldurulur; düğmeye basılınca parent UI'a bildirir.
/// Rarity renk çubuğu animasyonu DOTween yoksa coroutine ile çalışır.
/// </summary>
public class UpgradeCardUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image           _iconImage;
    [SerializeField] private TextMeshProUGUI _nameLabel;
    [SerializeField] private TextMeshProUGUI _descLabel;
    [SerializeField] private Image           _categoryColorBar;
    [SerializeField] private Image           _rarityBar;         // Alt rarity şerit
    [SerializeField] private TextMeshProUGUI _rarityLabel;       // "RARE", "EPIC" vs.
    [SerializeField] private Button          _button;

    [Header("Rarity Animasyon")]
    [SerializeField] private float _rarityPulseSpeed  = 1.8f;
    [SerializeField] private float _rarityPulseMin    = 0.6f;
    [SerializeField] private float _rarityPulseMax    = 1.0f;

    // Kategori renkleri
    private static readonly Color ColorGun       = new Color(1f,    0.42f, 0.42f);
    private static readonly Color ColorMovement  = new Color(0.42f, 0.71f, 1f);
    private static readonly Color ColorUtility   = new Color(0.75f, 0.42f, 1f);
    private static readonly Color ColorEvolution = new Color(1f,    0.84f, 0f);

    private UpgradeSelectionUI _parent;
    private UpgradeData        _data;
    private Coroutine          _pulseCoroutine;

    // ── API ───────────────────────────────────────────────────────────────

    public void Setup(UpgradeData data, UpgradeSelectionUI parent)
    {
        _data   = data;
        _parent = parent;

        if (_nameLabel != null) _nameLabel.text = data.UpgradeName;
        if (_descLabel != null) _descLabel.text  = data.Description;

        Color catColor = CategoryColor(data.Category);

        if (_iconImage != null)
        {
            _iconImage.sprite = data.Icon;
            _iconImage.color  = data.Icon != null ? Color.white : catColor;
        }

        if (_categoryColorBar != null)
            _categoryColorBar.color = catColor;

        // Rarity
        ApplyRarity(data.Rarity);

        if (_button != null)
        {
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(OnClick);
        }
    }

    // ── Rarity ────────────────────────────────────────────────────────────

    void ApplyRarity(UpgradeRarity rarity)
    {
        Color rarityColor = UpgradeRarityUtils.GetColor(rarity);

        if (_rarityBar != null)
            _rarityBar.color = rarityColor;

        if (_rarityLabel != null)
        {
            _rarityLabel.text  = UpgradeRarityUtils.GetDisplayName(rarity);
            _rarityLabel.color = rarityColor;
        }

        // Epic ve Rare kartlara pulse animasyonu
        if (_pulseCoroutine != null)
            StopCoroutine(_pulseCoroutine);

        if (rarity >= UpgradeRarity.Rare && _rarityBar != null)
            _pulseCoroutine = StartCoroutine(RarityPulse(rarityColor));
    }

    IEnumerator RarityPulse(Color baseColor)
    {
        float t = 0f;
        while (true)
        {
            t += Time.unscaledDeltaTime * _rarityPulseSpeed;
            float alpha = Mathf.Lerp(_rarityPulseMin, _rarityPulseMax,
                          (Mathf.Sin(t * Mathf.PI * 2f) + 1f) * 0.5f);

            if (_rarityBar != null)
                _rarityBar.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);

            yield return null;
        }
    }

    void OnDisable()
    {
        if (_pulseCoroutine != null)
        {
            StopCoroutine(_pulseCoroutine);
            _pulseCoroutine = null;
        }
    }

    // ── Internal ──────────────────────────────────────────────────────────

    void OnClick() => _parent?.OnCardSelected(_data);

    static Color CategoryColor(UpgradeCategory cat) => cat switch
    {
        UpgradeCategory.Gun       => ColorGun,
        UpgradeCategory.Movement  => ColorMovement,
        UpgradeCategory.Utility   => ColorUtility,
        UpgradeCategory.Evolution => ColorEvolution,
        _                         => Color.white,
    };
}
