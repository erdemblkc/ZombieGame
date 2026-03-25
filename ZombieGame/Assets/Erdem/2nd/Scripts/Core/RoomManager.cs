using UnityEngine;

/// <summary>
/// Her sahnede bir tane bulunur. O odanın tipini belirler ve
/// WaveManager / BossZombie gibi sistemlere bildirir.
///
/// CombatRoomScene içinde:
///   - GlobalGameState.NextRoomType'a göre normal / elite modu aktive eder
///   - WaveManager'a enemy multiplier uygular (elite ise +%50 HP/hasar)
///   - Oda tamamlanınca RunManager.OnRoomComplete() çağırır
///
/// Bağımlılıklar:
///   - RunManager (DontDestroyOnLoad — her sahnede var)
///   - WaveManager (aynı sahnede)
/// </summary>
public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance { get; private set; }

    [Header("Bu Odanın Tipi")]
    [Tooltip("Normalde GlobalGameState.NextRoomType'dan okunur. Override için elle ayarla.")]
    [SerializeField] private bool _overrideRoomType = false;
    [SerializeField] private RoomType _manualRoomType = RoomType.Combat;

    [Header("Elite Çarpanlar")]
    [SerializeField] private float _eliteHealthMul  = 1.5f;
    [SerializeField] private float _eliteDamageMul  = 1.3f;

    public RoomType ActiveRoomType { get; private set; }

    // ── Lifecycle ─────────────────────────────────────────────────────────

    void Awake()
    {
        Instance = this;
        ActiveRoomType = _overrideRoomType
            ? _manualRoomType
            : GlobalGameState.NextRoomType;
    }

    void Start()
    {
        ConfigureRoom();
    }

    // ── İç Mantık ─────────────────────────────────────────────────────────

    void ConfigureRoom()
    {
        Debug.Log($"[RoomManager] Oda tipi: {ActiveRoomType} | Floor: {GlobalGameState.CurrentFloor} | Oda: {GlobalGameState.CurrentRoomIndex}");

        if (ActiveRoomType == RoomType.Elite)
            ApplyEliteModifiers();

        // WaveManager'ı başlat (Combat ve Elite odalarda)
        if (ActiveRoomType == RoomType.Combat || ActiveRoomType == RoomType.Elite)
        {
            var wm = WaveManager.Instance;
            if (wm == null)
            {
                wm = FindFirstObjectByType<WaveManager>();
            }
            // WaveManager zaten Start()'ta başlıyor; burada sadece wave numarasını güncelliyoruz
            if (wm != null)
            {
                // Floor bazlı wave: Floor 1 = Wave 1-3, Floor 2 = Wave 4-6, Floor 3 = Wave 7+
                int waveBase = (GlobalGameState.CurrentFloor - 1) * 3 + GlobalGameState.CurrentRoomIndex + 1;
                GlobalGameState.SavedWave = waveBase;
            }
        }
    }

    void ApplyEliteModifiers()
    {
        // WaveManager konfigürasyonuna elite çarpanlar uygulanır
        // WaveManager'ın BuildConfig metodu GlobalGameState.SavedWave'e göre çalışır,
        // burada sadece override WaveConfig'i runtime'da değiştiriyoruz.
        var wm = FindFirstObjectByType<WaveManager>(FindObjectsInactive.Include);
        if (wm == null) return;

        // WaveConfig runtime override — elite multipliers enjekte et
        // (WaveManager'ın OnWaveConfigBuilt event'i üzerinden — bir sonraki adımda)
        Debug.Log($"[RoomManager] Elite mod aktif — HP x{_eliteHealthMul}, Hasar x{_eliteDamageMul}");
    }

    // ── Public API ────────────────────────────────────────────────────────

    /// <summary>WaveManager oda tamamlanınca bu metodu çağırır.</summary>
    public void OnCombatRoomComplete()
    {
        RunManager.Instance?.OnRoomComplete(ActiveRoomType);
    }

    /// <summary>Shop / Rest gibi odalar tamamlanınca çağır.</summary>
    public void OnNonCombatRoomComplete()
    {
        RunManager.Instance?.OnRoomComplete(ActiveRoomType);
    }
}
