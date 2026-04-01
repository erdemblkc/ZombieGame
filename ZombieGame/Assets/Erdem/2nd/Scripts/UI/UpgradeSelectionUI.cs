using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

/// <summary>
/// Wave sonu upgrade seçim ekranı.
/// WaveManager, AllSlotsFull() false ise Show() çağırır.
/// Oyuncu bir kart seçince UpgradeSlotManager.AddUpgrade() yapılır,
/// ekran kapanır ve WaveManager.StartNextWave() çağrılır.
///
/// Sahne kurulumu:
///   1. Canvas altında bir Panel (UpgradeSelectionPanel) oluştur.
///   2. Bu scripti Panel'e ekle.
///   3. _cardContainer içinde 4 UpgradeCard prefab instance'ı olacak (veya runtime spawn edilir).
///   4. _allUpgrades dizisini Inspector'dan doldur — tüm upgrade asset'lerini buraya ekle.
///   5. _cardPrefab alanına UpgradeCard prefabını ata.
/// </summary>
public class UpgradeSelectionUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject      _panel;
    [SerializeField] private CanvasGroup     _canvasGroup;
    [SerializeField] private TextMeshProUGUI _titleLabel;

    [Header("Cards")]
    [Tooltip("UpgradeCard prefab — 4 tane spawn edilecek.")]
    [SerializeField] private UpgradeCardUI   _cardPrefab;
    [SerializeField] private Transform       _cardContainer;

    [Header("Upgrade Pool")]
    [Tooltip("Tüm mevcut upgrade asset'lerini bu diziye ekle.")]
    [SerializeField] private UpgradeData[]   _allUpgrades;

    [Header("Animation")]
    [SerializeField] private float _fadeInDuration = 0.35f;

    private readonly List<UpgradeCardUI> _spawnedCards = new List<UpgradeCardUI>();

    // ── Lifecycle ──────────────────────────────────────────────────────────

    void Awake()
    {
        if (_canvasGroup == null && _panel != null)
            _canvasGroup = _panel.GetComponent<CanvasGroup>() ?? _panel.AddComponent<CanvasGroup>();

        // HideImmediate() BURAYA YAZILMAZ.
        // Panel sahnede inactive olarak kaydedildiği için Awake scene load'da çalışmaz.
        // WaveManager.Show() → _panel.SetActive(true) → Awake tetiklenir (senkron) →
        // HideImmediate() → SetActive(false) → panel anında kapanır (BUG).
        // Bunun yerine CanvasGroup alpha'yı 0'a ayarla; DOTween zaten 0'dan fade-in yapacak.
        if (_canvasGroup != null) _canvasGroup.alpha = 0f;
    }

    // ── Public API ────────────────────────────────────────────────────────

    /// <summary>
    /// Oyunu durdurur, havuzdan 4 upgrade seçer ve kartları gösterir.
    /// WaveManager tarafından çağrılır.
    /// </summary>
    public void Show(int completedWaveNumber)
    {
        if (_panel == null) { Debug.LogError("[UpgradeSelectionUI] Panel reference is NULL."); return; }

        if (_titleLabel != null)
            _titleLabel.text = "Bir Upgrade Seç";

        PopulateCards();

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        _panel.SetActive(true);
        _canvasGroup.alpha          = 0f;
        _canvasGroup.interactable   = true;
        _canvasGroup.blocksRaycasts = true;
        _panel.transform.localScale = Vector3.one * 0.9f;

        DOTween.Sequence()
            .SetUpdate(true)
            .Append(_canvasGroup.DOFade(1f, _fadeInDuration).SetEase(Ease.OutQuad))
            .Join(_panel.transform.DOScale(Vector3.one, _fadeInDuration).SetEase(Ease.OutBack));
    }

    /// <summary>Called by UpgradeCardUI when a card is clicked.</summary>
    public void OnCardSelected(UpgradeData selected)
    {
        if (selected == null) return;

        // Add to first available slot
        var slotManager = UpgradeSlotManager.Instance;
        if (slotManager != null)
            slotManager.AddUpgrade(selected);

        Close();
        WaveManager.Instance?.StartNextWave();
    }

    // ── Internal ──────────────────────────────────────────────────────────

    void PopulateCards()
    {
        // Clear old cards
        foreach (var c in _spawnedCards)
            if (c != null) Destroy(c.gameObject);
        _spawnedCards.Clear();

        List<UpgradeData> picks = PickUpgrades(4);

        foreach (var data in picks)
        {
            if (_cardPrefab == null) break;
            UpgradeCardUI card = Instantiate(_cardPrefab, _cardContainer);
            card.Setup(data, this);
            _spawnedCards.Add(card);
        }
    }

    /// <summary>
    /// Havuzdan uygun upgrades seçer:
    ///   - Oyuncu zaten sahipse hariç tut.
    ///   - Evolution ise her iki prerequisite slotta olmalı.
    ///   - Rastgele karıştır, ilk maxCount tanesini döndür.
    /// </summary>
    List<UpgradeData> PickUpgrades(int maxCount)
    {
        if (_allUpgrades == null || _allUpgrades.Length == 0)
        {
            Debug.LogWarning("[UpgradeSelectionUI] _allUpgrades dizisi boş — Inspector'dan doldur.");
            return new List<UpgradeData>();
        }

        var slotManager = UpgradeSlotManager.Instance;
        var available   = new List<UpgradeData>();

        foreach (var data in _allUpgrades)
        {
            if (data == null) continue;

            // Skip if already equipped
            if (slotManager != null && slotManager.HasUpgrade(data)) continue;

            // For evolutions: all prerequisites must be present
            if (data.Category == UpgradeCategory.Evolution)
            {
                if (data.Prerequisites == null || data.Prerequisites.Length == 0) continue;
                bool allPrereqsMet = true;
                foreach (var prereq in data.Prerequisites)
                {
                    if (prereq == null || slotManager == null || !slotManager.HasUpgrade(prereq))
                    {
                        allPrereqsMet = false;
                        break;
                    }
                }
                if (!allPrereqsMet) continue;
            }

            available.Add(data);
        }

        // Fisher-Yates shuffle
        for (int i = available.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (available[i], available[j]) = (available[j], available[i]);
        }

        return available.Count > maxCount ? available.GetRange(0, maxCount) : available;
    }

    void Close()
    {
        DOTween.Kill(_canvasGroup);
        _canvasGroup.DOFade(0f, 0.2f)
            .SetUpdate(true)
            .OnComplete(HideImmediate);

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
    }

    void HideImmediate()
    {
        if (_panel != null) _panel.SetActive(false);
    }
}
