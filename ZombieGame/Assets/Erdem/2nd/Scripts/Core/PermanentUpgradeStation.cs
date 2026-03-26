using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// HUB sahnesi — Serum ile kalıcı upgrade satın alma istasyonu.
///
/// Kurulum:
///   1. HubScene'de boş bir GameObject'e ekle.
///   2. _upgrades listesine PermanentUpgradeData asset'lerini ata.
///   3. UI referanslarını bağla (veya Inspector'dan doldur).
///   4. MetaCurrencyManager sahnede var olmalı.
///
/// PlayerPrefs key formatı: "PermanentUpgrade_{upgradeId}_Level"
/// </summary>
public class PermanentUpgradeStation : MonoBehaviour
{
    public static PermanentUpgradeStation Instance { get; private set; }

    // ── Inspector ─────────────────────────────────────────────────────────

    [Header("Upgrade Listesi")]
    [SerializeField] private PermanentUpgradeData[] _upgrades;

    [Header("UI (Opsiyonel — Erdem bağlayacak)")]
    [SerializeField] private GameObject        _stationPanel;
    [SerializeField] private Transform         _cardContainer;
    [SerializeField] private GameObject        _cardPrefab;
    [SerializeField] private TextMeshProUGUI   _serumLabel;
    [SerializeField] private Button            _closeButton;

    [Header("Events")]
    public UnityEvent<PermanentUpgradeData> OnUpgradePurchased = new UnityEvent<PermanentUpgradeData>();

    // ── Internal ──────────────────────────────────────────────────────────

    readonly Dictionary<string, int> _levels = new Dictionary<string, int>();

    // ── Lifecycle ─────────────────────────────────────────────────────────

    void Awake()
    {
        Instance = this;
        LoadAllLevels();
    }

    void Start()
    {
        if (_closeButton != null)
            _closeButton.onClick.AddListener(CloseStation);

        if (_stationPanel != null)
            _stationPanel.SetActive(false);

        // MetaCurrencyManager Serum değişince label'ı güncelle
        if (MetaCurrencyManager.Instance != null)
            MetaCurrencyManager.Instance.OnSerumChanged.AddListener(UpdateSerumLabel);

        UpdateSerumLabel(GlobalGameState.MetaCurrency);
        ApplyAllUpgrades(); // Run başlangıcı için mevcut yükseltmeleri uygula
    }

    // ── Public API ────────────────────────────────────────────────────────

    public void OpenStation()
    {
        if (_stationPanel != null) _stationPanel.SetActive(true);
        BuildUI();
        UpdateSerumLabel(GlobalGameState.MetaCurrency);
    }

    public void CloseStation()
    {
        if (_stationPanel != null) _stationPanel.SetActive(false);
    }

    /// <summary>Upgrade satın al. Yetersiz Serum veya max level'da false döner.</summary>
    public bool Purchase(PermanentUpgradeData data)
    {
        int currentLevel = GetLevel(data.upgradeId);

        if (currentLevel >= data.maxLevel)
        {
            Debug.Log($"[PermanentUpgradeStation] '{data.upgradeName}' zaten max seviyede.");
            return false;
        }

        if (!MetaCurrencyManager.Instance.SpendSerum(data.serumCost))
            return false;

        // Seviyeyi artır ve kaydet
        _levels[data.upgradeId] = currentLevel + 1;
        SaveLevel(data.upgradeId, _levels[data.upgradeId]);

        // Etkiyi uygula
        ApplyUpgrade(data, _levels[data.upgradeId]);

        Debug.Log($"[PermanentUpgradeStation] '{data.upgradeName}' satın alındı. Seviye: {_levels[data.upgradeId]}/{data.maxLevel}");
        OnUpgradePurchased?.Invoke(data);

        BuildUI(); // Kartları güncelle
        return true;
    }

    /// <summary>Belirli bir upgrade'in mevcut seviyesini döner.</summary>
    public int GetLevel(string upgradeId)
    {
        return _levels.TryGetValue(upgradeId, out int lvl) ? lvl : 0;
    }

    /// <summary>Upgrade satın alınmış mı?</summary>
    public bool IsOwned(string upgradeId) => GetLevel(upgradeId) > 0;

    // ── Etki Uygulama ─────────────────────────────────────────────────────

    /// <summary>Tüm satın alınmış upgrade'leri GlobalGameState'e uygula.</summary>
    public void ApplyAllUpgrades()
    {
        if (_upgrades == null) return;
        foreach (var data in _upgrades)
        {
            int level = GetLevel(data.upgradeId);
            if (level > 0)
                ApplyUpgrade(data, level);
        }
    }

    void ApplyUpgrade(PermanentUpgradeData data, int level)
    {
        float totalValue = data.effectValue * level;

        switch (data.effect)
        {
            case PermanentUpgradeEffect.MaxHPBonus:
                GlobalGameState.PermanentMaxHPBonus += (int)totalValue;
                break;

            case PermanentUpgradeEffect.StartingHPPercent:
                GlobalGameState.PermanentStartingHPBonus += totalValue;
                break;

            case PermanentUpgradeEffect.DamagePercent:
                GlobalGameState.PermanentDamageBonus += totalValue;
                break;

            case PermanentUpgradeEffect.InfectionSlowPercent:
                GlobalGameState.PermanentInfectionSlowBonus += totalValue;
                break;

            case PermanentUpgradeEffect.ExtraUpgradeChoice:
                GlobalGameState.PermanentExtraUpgradeChoices += (int)totalValue;
                break;

            case PermanentUpgradeEffect.SerumBonusPercent:
                GlobalGameState.PermanentSerumBonus += totalValue;
                break;

            default:
                Debug.Log($"[PermanentUpgradeStation] '{data.effect}' etkisi GlobalGameState'de henüz uygulanmıyor.");
                break;
        }
    }

    // ── UI ────────────────────────────────────────────────────────────────

    void BuildUI()
    {
        if (_cardContainer == null || _cardPrefab == null || _upgrades == null) return;

        // Mevcut kartları temizle
        foreach (Transform child in _cardContainer)
            Destroy(child.gameObject);

        foreach (var data in _upgrades)
        {
            var card = Instantiate(_cardPrefab, _cardContainer);
            SetupCard(card, data);
        }
    }

    void SetupCard(GameObject card, PermanentUpgradeData data)
    {
        int level       = GetLevel(data.upgradeId);
        bool maxed      = level >= data.maxLevel;
        bool canAfford  = MetaCurrencyManager.Instance != null &&
                          MetaCurrencyManager.Instance.CanAfford(data.serumCost);

        // İsim
        var nameLabel = card.GetComponentInChildren<TextMeshProUGUI>();
        if (nameLabel != null)
            nameLabel.text = maxed
                ? $"{data.upgradeName} (MAX)"
                : $"{data.upgradeName} — {data.serumCost} Serum";

        // Buton
        var button = card.GetComponentInChildren<Button>();
        if (button != null)
        {
            button.interactable = !maxed && canAfford;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => Purchase(data));
        }

        // İkon
        var icon = card.GetComponentInChildren<Image>();
        if (icon != null && data.icon != null)
            icon.sprite = data.icon;
    }

    void UpdateSerumLabel(int amount)
    {
        if (_serumLabel != null)
            _serumLabel.text = $"Serum: {amount}";
    }

    // ── Save / Load ───────────────────────────────────────────────────────

    void LoadAllLevels()
    {
        if (_upgrades == null) return;
        foreach (var data in _upgrades)
        {
            string key = PrefsKey(data.upgradeId);
            _levels[data.upgradeId] = PlayerPrefs.GetInt(key, 0);
        }
    }

    void SaveLevel(string upgradeId, int level)
    {
        PlayerPrefs.SetInt(PrefsKey(upgradeId), level);
        PlayerPrefs.Save();
    }

    static string PrefsKey(string upgradeId) => $"PermanentUpgrade_{upgradeId}_Level";

    /// <summary>Debug — tüm kalıcı upgrade'leri sıfırla.</summary>
    [ContextMenu("RESET ALL PERMANENT UPGRADES")]
    public void ResetAll()
    {
        if (_upgrades == null) return;
        foreach (var data in _upgrades)
        {
            PlayerPrefs.DeleteKey(PrefsKey(data.upgradeId));
            _levels[data.upgradeId] = 0;
        }
        GlobalGameState.ResetPermanentBonuses();
        Debug.Log("[PermanentUpgradeStation] Tüm kalıcı upgrade'ler sıfırlandı.");
    }
}
