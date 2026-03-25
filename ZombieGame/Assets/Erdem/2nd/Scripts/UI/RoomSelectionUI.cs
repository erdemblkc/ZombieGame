using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

/// <summary>
/// Oda seçim ekranı. RunManager.SelectRoom(RoomType) ile sahneyi yükler.
///
/// Hierarchy:
///   Canvas
///     RoomSelectionPanel (bu script)
///       Title (TMP)
///       RoomChoicesContainer (HorizontalLayoutGroup)
///         RoomCard_0
///         RoomCard_1
///         RoomCard_2
///
/// Her kart:
///   - Icon (Image) — oda tipi ikonu
///   - RoomTypeName (TMP) — oda adı
///   - RoomDescription (TMP) — kısa açıklama
///   - SelectButton (Button)
/// </summary>
public class RoomSelectionUI : MonoBehaviour
{
    public static RoomSelectionUI Instance { get; private set; }

    [Header("Panel")]
    [SerializeField] private GameObject  _panel;
    [SerializeField] private CanvasGroup _canvasGroup;

    [Header("Kart Referansları (Inspector'dan ata — 2 veya 3)")]
    [SerializeField] private RoomCardUI[] _cards;

    [Header("Animasyon")]
    [SerializeField] private float _fadeInDuration  = 0.4f;
    [SerializeField] private float _cardDelay       = 0.1f;

    void Awake()
    {
        Instance = this;

        if (_canvasGroup == null && _panel != null)
            _canvasGroup = _panel.GetComponent<CanvasGroup>() ?? _panel.AddComponent<CanvasGroup>();

        if (_canvasGroup != null) _canvasGroup.alpha = 0f;
    }

    void Start()
    {
        // Bu sahne yüklenince direkt göster
        Show();
    }

    // ── Public API ────────────────────────────────────────────────────────

    public void Show()
    {
        if (_panel == null) return;

        RoomType[] choices = GenerateChoices();
        for (int i = 0; i < _cards.Length; i++)
        {
            if (i < choices.Length)
            {
                _cards[i].gameObject.SetActive(true);
                _cards[i].Setup(choices[i], OnCardSelected);
            }
            else
            {
                _cards[i].gameObject.SetActive(false);
            }
        }

        _panel.SetActive(true);
        _canvasGroup.alpha = 0f;
        _canvasGroup.DOFade(1f, _fadeInDuration);

        // Kartları sırayla animasyon
        for (int i = 0; i < _cards.Length; i++)
        {
            if (!_cards[i].gameObject.activeSelf) continue;
            var rect = _cards[i].GetComponent<RectTransform>();
            if (rect == null) continue;
            Vector3 orig = rect.localPosition;
            rect.localPosition = orig + Vector3.down * 40f;
            rect.DOLocalMove(orig, 0.3f).SetDelay(_cardDelay * i).SetEase(Ease.OutBack);
        }
    }

    void OnCardSelected(RoomType roomType)
    {
        _canvasGroup.DOFade(0f, 0.2f).OnComplete(() =>
        {
            RunManager.Instance?.SelectRoom(roomType);
        });
    }

    // ── Oda Seçenekleri ───────────────────────────────────────────────────

    /// <summary>3 seçenek üret: 1 garantili Combat, diğerleri ağırlıklı rastgele.</summary>
    RoomType[] GenerateChoices()
    {
        int floor = GlobalGameState.CurrentFloor;

        // Combat her zaman dahil
        var pool = new System.Collections.Generic.List<RoomType> { RoomType.Combat };

        // İkinci seçenek: floor 1'de Elite şansı düşük
        float eliteChance = 0.2f + (floor - 1) * 0.15f;
        pool.Add(Random.value < eliteChance ? RoomType.Elite : RoomType.Shop);

        // Üçüncü seçenek: Rest veya Event
        pool.Add(Random.value < 0.6f ? RoomType.Rest : RoomType.Event);

        // Karıştır
        for (int i = pool.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (pool[i], pool[j]) = (pool[j], pool[i]);
        }

        return pool.ToArray();
    }
}

// ──────────────────────────────────────────────────────────────────────────

/// <summary>Tek bir oda kartı görünümü.</summary>
[System.Serializable]
public class RoomCardUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _roomNameText;
    [SerializeField] private TextMeshProUGUI _descriptionText;
    [SerializeField] private Image           _iconImage;
    [SerializeField] private Button          _selectButton;

    private RoomType _roomType;
    private System.Action<RoomType> _onSelected;

    public void Setup(RoomType type, System.Action<RoomType> onSelected)
    {
        _roomType   = type;
        _onSelected = onSelected;

        var (name, desc, color) = GetRoomInfo(type);
        if (_roomNameText    != null) _roomNameText.text    = name;
        if (_descriptionText != null) _descriptionText.text = desc;
        if (_iconImage       != null) _iconImage.color      = color;

        _selectButton?.onClick.RemoveAllListeners();
        _selectButton?.onClick.AddListener(() => _onSelected?.Invoke(_roomType));
    }

    static (string name, string desc, Color color) GetRoomInfo(RoomType type) => type switch
    {
        RoomType.Combat => ("⚔ SAVAŞ",   "Zombileri öldür\nUpgrade kazan",          new Color(0.9f, 0.3f, 0.3f)),
        RoomType.Elite  => ("⚠ ELİTE",   "Güçlü zombiler\nEkstra ödül",             new Color(1.0f, 0.6f, 0.1f)),
        RoomType.Shop   => ("🛒 MARKET",  "Run currency ile\nUpgrade satın al",      new Color(0.3f, 0.8f, 0.4f)),
        RoomType.Rest   => ("💉 DİNLENME","HP doldur VEYA\nEnfeksiyon azalt",        new Color(0.3f, 0.6f, 1.0f)),
        RoomType.Event  => ("❓ OLAY",    "Risk / Ödül seçimi\nSürpriz içerik",      new Color(0.7f, 0.4f, 1.0f)),
        RoomType.Boss   => ("💀 BOSS",    "Bölüm sonu boss\nNadir upgrade ödülü",    new Color(0.8f, 0.1f, 0.1f)),
        _               => ("ODA",        "",                                          Color.white),
    };
}
