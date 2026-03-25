using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Roguelike run akışını yönetir. Singleton.
///
/// Sorumluluklar:
///   - Run başlatma / bitirme
///   - Floor ve oda sırası takibi
///   - Run currency toplama ve harcama
///   - Meta currency (Serum) kazanımı
///   - Oda tamamlanınca RoomSelectionScene'e geçiş
///   - Ölünce / run bitince HubScene'e geçiş
///
/// Sahne Akışı:
///   HubScene → CombatRoomScene → RoomSelectionScene → [seçilen oda] → ... → BossScene → HubScene
/// </summary>
public class RunManager : MonoBehaviour
{
    public static RunManager Instance { get; private set; }

    // ── Sahne İsimleri ────────────────────────────────────────────────────
    [Header("Sahne İsimleri")]
    [SerializeField] private string _hubSceneName           = "HubScene";
    [SerializeField] private string _combatRoomSceneName    = "CombatRoomScene";
    [SerializeField] private string _roomSelectionSceneName = "RoomSelectionScene";
    [SerializeField] private string _shopSceneName          = "ShopScene";
    [SerializeField] private string _restSceneName          = "RestScene";
    [SerializeField] private string _bossSceneName          = "BossScene";

    // ── Bölüm Yapısı ─────────────────────────────────────────────────────
    [Header("Bölüm Yapısı")]
    [Tooltip("Her bölümde kaç normal oda var (boss hariç)?")]
    [SerializeField] private int _combatRoomsPerFloor = 3;
    [Tooltip("Toplam kaç bölüm var?")]
    [SerializeField] private int _totalFloors = 3;

    // ── Run Currency Kazanımı ─────────────────────────────────────────────
    [Header("Currency")]
    [Tooltip("Combat oda tamamlanınca kaç RunCurrency kazanılır?")]
    [SerializeField] private int _currencyPerCombatRoom = 10;
    [Tooltip("Elite oda bonus kazanımı.")]
    [SerializeField] private int _currencyBonusElite    = 15;
    [Tooltip("Boss öldürünce kazanım.")]
    [SerializeField] private int _currencyPerBoss       = 50;
    [Tooltip("Run sonu: RunCurrency'nin yüzde kaçı Serum'a dönüşür?")]
    [SerializeField][Range(0f, 1f)] private float _serumConversionRate = 0.5f;

    // ── State ─────────────────────────────────────────────────────────────
    public int  CurrentFloor     => GlobalGameState.CurrentFloor;
    public int  CurrentRoomIndex => GlobalGameState.CurrentRoomIndex;
    public int  RunCurrency      => GlobalGameState.RunCurrency;
    public bool RunActive        { get; private set; }

    // ── Lifecycle ─────────────────────────────────────────────────────────

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ── Public API ────────────────────────────────────────────────────────

    /// <summary>HubScene'den run başlatırken çağır.</summary>
    public void StartRun()
    {
        GlobalGameState.StartNewRun();
        RunActive = true;
        Debug.Log("[RunManager] Run başladı — Floor 1.");
        LoadScene(_combatRoomSceneName);
    }

    /// <summary>Oda tamamlanınca çağır (WaveManager veya BossZombie tarafından).</summary>
    public void OnRoomComplete(RoomType completedType)
    {
        if (!RunActive) return;

        // Currency kazan
        int earned = completedType switch
        {
            RoomType.Elite => _currencyPerCombatRoom + _currencyBonusElite,
            RoomType.Boss  => _currencyPerBoss,
            _              => _currencyPerCombatRoom,
        };
        AddRunCurrency(earned);
        Debug.Log($"[RunManager] Oda tamamlandı ({completedType}). +{earned} currency. Toplam: {RunCurrency}");

        if (completedType == RoomType.Boss)
        {
            OnBossDefeated();
            return;
        }

        GlobalGameState.CurrentRoomIndex++;

        // Bölüm sonu boss'a mı gidiyoruz?
        if (GlobalGameState.CurrentRoomIndex >= _combatRoomsPerFloor)
        {
            Debug.Log($"[RunManager] Floor {CurrentFloor} boss odasına geçiliyor.");
            GlobalGameState.NextRoomType = RoomType.Boss;
            LoadScene(_bossSceneName);
        }
        else
        {
            // Oda seçim ekranına git
            LoadScene(_roomSelectionSceneName);
        }
    }

    /// <summary>Boss yenilince çağrılır.</summary>
    void OnBossDefeated()
    {
        Debug.Log($"[RunManager] Floor {CurrentFloor} boss yenildi!");

        if (GlobalGameState.CurrentFloor >= _totalFloors)
        {
            // Oyun kazanıldı!
            OnRunVictory();
        }
        else
        {
            GlobalGameState.CurrentFloor++;
            GlobalGameState.CurrentRoomIndex = 0;
            Debug.Log($"[RunManager] Floor {CurrentFloor}'e geçildi.");
            // Sonraki floor başlangıcı — oda seçiminden başla
            LoadScene(_roomSelectionSceneName);
        }
    }

    /// <summary>Oyuncu ölünce çağır (PlayerDamageReceiver veya InfectionSystem).</summary>
    public void OnPlayerDied()
    {
        if (!RunActive) return;
        RunActive = false;

        // Run currency'nin bir kısmını Serum'a çevir
        int serumEarned = Mathf.RoundToInt(RunCurrency * _serumConversionRate);
        GlobalGameState.MetaCurrency += serumEarned;
        Debug.Log($"[RunManager] Oyuncu öldü. {serumEarned} Serum kazanıldı. Toplam: {GlobalGameState.MetaCurrency}");

        LoadScene(_hubSceneName);
    }

    /// <summary>Run kazanıldı.</summary>
    void OnRunVictory()
    {
        RunActive = false;
        // Kazanılan Serum miktarı ölümden daha yüksek
        int serumEarned = Mathf.RoundToInt(RunCurrency * (_serumConversionRate * 2f));
        GlobalGameState.MetaCurrency += serumEarned;
        Debug.Log($"[RunManager] RUN KAZANILDI! {serumEarned} Serum. Toplam: {GlobalGameState.MetaCurrency}");
        LoadScene(_hubSceneName);
    }

    /// <summary>Seçilen oda tipini kaydet ve o sahneye git.</summary>
    public void SelectRoom(RoomType roomType)
    {
        GlobalGameState.NextRoomType = roomType;
        string sceneName = roomType switch
        {
            RoomType.Shop  => _shopSceneName,
            RoomType.Rest  => _restSceneName,
            RoomType.Elite => _combatRoomSceneName,  // Elite de combat sahnesi, WaveManager farkı ayarlar
            _              => _combatRoomSceneName,
        };
        LoadScene(sceneName);
    }

    /// <summary>Run currency ekle.</summary>
    public void AddRunCurrency(int amount)
    {
        GlobalGameState.RunCurrency = Mathf.Max(0, GlobalGameState.RunCurrency + amount);
    }

    /// <summary>Run currency harca. Yetersizse false döner.</summary>
    public bool SpendRunCurrency(int amount)
    {
        if (GlobalGameState.RunCurrency < amount) return false;
        GlobalGameState.RunCurrency -= amount;
        return true;
    }

    // ── Internal ──────────────────────────────────────────────────────────

    void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
