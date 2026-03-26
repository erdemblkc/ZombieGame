using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Serum (meta currency) yönetimi — PlayerPrefs ile kalıcı kayıt.
/// Run dışında HubScene'de çalışır. RunManager ölünce/kazanınca Serum ekler.
///
/// Bağımlılıklar:
///   - GlobalGameState.MetaCurrency (runtime değer)
///   - RunManager (Serum kazanımını tetikler)
///   - PermanentUpgradeStation (harcama için)
/// </summary>
public class MetaCurrencyManager : MonoBehaviour
{
    public static MetaCurrencyManager Instance { get; private set; }

    // ── PlayerPrefs Key ───────────────────────────────────────────────────

    const string PREFS_KEY_SERUM    = "MetaCurrency_Serum";
    const string PREFS_KEY_RUNS     = "Stats_TotalRuns";
    const string PREFS_KEY_WINS     = "Stats_TotalWins";
    const string PREFS_KEY_BOSSES   = "Stats_BossesDefeated";

    // ── Events ────────────────────────────────────────────────────────────

    /// <summary>Serum miktarı değişince tetiklenir. int = yeni miktar.</summary>
    public UnityEvent<int> OnSerumChanged = new UnityEvent<int>();

    // ── Properties ────────────────────────────────────────────────────────

    public int TotalSerum       => GlobalGameState.MetaCurrency;
    public int TotalRuns        { get; private set; }
    public int TotalWins        { get; private set; }
    public int TotalBossDefeats { get; private set; }

    // ── Lifecycle ─────────────────────────────────────────────────────────

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    void OnApplicationQuit()
    {
        Save();
    }

    // ── Public API ────────────────────────────────────────────────────────

    /// <summary>Serum ekle ve kaydet.</summary>
    public void AddSerum(int amount)
    {
        if (amount <= 0) return;
        GlobalGameState.MetaCurrency += amount;
        Debug.Log($"[MetaCurrencyManager] +{amount} Serum. Toplam: {TotalSerum}");
        OnSerumChanged?.Invoke(TotalSerum);
        Save();
    }

    /// <summary>Serum harca. Yetersizse false döner.</summary>
    public bool SpendSerum(int amount)
    {
        if (GlobalGameState.MetaCurrency < amount)
        {
            Debug.Log($"[MetaCurrencyManager] Yetersiz Serum! Gerekli: {amount}, Mevcut: {TotalSerum}");
            return false;
        }
        GlobalGameState.MetaCurrency -= amount;
        Debug.Log($"[MetaCurrencyManager] -{amount} Serum harcandı. Kalan: {TotalSerum}");
        OnSerumChanged?.Invoke(TotalSerum);
        Save();
        return true;
    }

    /// <summary>Yeterli Serum var mı?</summary>
    public bool CanAfford(int cost) => TotalSerum >= cost;

    // ── İstatistik Takibi ─────────────────────────────────────────────────

    public void RecordRunEnd(bool victory)
    {
        TotalRuns++;
        if (victory) TotalWins++;
        Save();
    }

    public void RecordBossDefeated()
    {
        TotalBossDefeats++;
        Save();
    }

    // ── Save / Load ───────────────────────────────────────────────────────

    public void Save()
    {
        PlayerPrefs.SetInt(PREFS_KEY_SERUM,   GlobalGameState.MetaCurrency);
        PlayerPrefs.SetInt(PREFS_KEY_RUNS,    TotalRuns);
        PlayerPrefs.SetInt(PREFS_KEY_WINS,    TotalWins);
        PlayerPrefs.SetInt(PREFS_KEY_BOSSES,  TotalBossDefeats);
        PlayerPrefs.Save();
    }

    void Load()
    {
        GlobalGameState.MetaCurrency = PlayerPrefs.GetInt(PREFS_KEY_SERUM,  0);
        TotalRuns                    = PlayerPrefs.GetInt(PREFS_KEY_RUNS,   0);
        TotalWins                    = PlayerPrefs.GetInt(PREFS_KEY_WINS,   0);
        TotalBossDefeats             = PlayerPrefs.GetInt(PREFS_KEY_BOSSES, 0);

        Debug.Log($"[MetaCurrencyManager] Yüklendi — Serum: {TotalSerum}, Run: {TotalRuns}, Kazanma: {TotalWins}");
    }

    /// <summary>Debug — tüm kayıtları sıfırla (sadece editörde kullan).</summary>
    [ContextMenu("RESET ALL META DATA")]
    public void ResetAll()
    {
        PlayerPrefs.DeleteKey(PREFS_KEY_SERUM);
        PlayerPrefs.DeleteKey(PREFS_KEY_RUNS);
        PlayerPrefs.DeleteKey(PREFS_KEY_WINS);
        PlayerPrefs.DeleteKey(PREFS_KEY_BOSSES);
        GlobalGameState.MetaCurrency = 0;
        TotalRuns = TotalWins = TotalBossDefeats = 0;
        OnSerumChanged?.Invoke(0);
        Debug.Log("[MetaCurrencyManager] Tüm meta veriler sıfırlandı.");
    }
}
