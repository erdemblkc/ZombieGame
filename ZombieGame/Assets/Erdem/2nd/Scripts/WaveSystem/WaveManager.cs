using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Ana wave controller. EnemySpawner ile çalışarak wave dizisini yönetir.
///
/// KURULUM (hızlı — ScriptableObject gerekmez):
///   1. Bu scripti sahnede bir GameObject'e ekle.
///   2. _wave1EnemyPrefab ve _wave2EnemyPrefab referanslarını ata.
///   3. EnemySpawner referansını ata (aynı veya başka bir GameObject).
///   4. WaveHUD ve UpgradeSelectionUI referanslarını ata.
///   5. _waveConfigs dizisini BOŞ bırak → 5 dahili wave + sonsuz mod çalışır.
///      Veya doldur → kendi wave'lerini kullanır.
///   6. ZombieSpawner1'i deactivate et.
///
/// DAHİLİ 5 WAVE (prefab atamasından sonra otomatik çalışır):
///   Wave 1 →  8 zombi,  temel hız
///   Wave 2 → 12 zombi, +%20 hız, +%25 HP
///   Wave 3 → 16 zombi, +%35 hız, +%50 HP, +%30 hasar (wave2 prefab)
///   Wave 4 → 20 zombi, +%50 hız, +%80 HP, +%50 hasar (wave2 prefab)
///   Wave 5 → 25 zombi, +%70 hız, +%120 HP, +%80 hasar (wave2 prefab)
///   Wave 6+→ otomatik skalalar
/// </summary>
public class WaveManager : MonoBehaviour
{
    // ── Singleton ────────────────────────────────────────────────────────
    public static WaveManager Instance { get; private set; }

    // ── Inspector: Wave Data ──────────────────────────────────────────────
    [Header("Wave Data (opsiyonel — boş bırakırsan dahili 5 wave kullanılır)")]
    [Tooltip("Boş bırakılırsa dahili wave tanımları devreye girer.")]
    [SerializeField] private WaveConfig[] _waveConfigs;

    [Header("Prefab Referansları (dahili wave modu için zorunlu)")]
    [Tooltip("Wave 1-2 için temel zombi prefabı.")]
    [SerializeField] private GameObject _wave1EnemyPrefab;
    [Tooltip("Wave 3+ için güçlü zombi prefabı. Atanmazsa wave1 prefabı kullanılır.")]
    [SerializeField] private GameObject _wave2EnemyPrefab;

    [Header("Sonsuz Mod (Wave 6+)")]
    [Tooltip("Wave 6'dan itibaren her wave'de bu kadar zombi eklenir.")]
    [SerializeField] private int _enemyCountIncreasePerWave = 3;
    [Tooltip("Spawn arası bekleme süresi (saniye).")]
    [SerializeField] private float _timeBetweenSpawns = 0.4f;

    [Header("Referanslar")]
    [SerializeField] private EnemySpawner       _spawner;
    [SerializeField] private UpgradeSelectionUI _upgradeSelectionUI;
    [SerializeField] private WaveHUD            _waveHUD;

    [Header("Zamanlama")]
    [Tooltip("Slotlar doluysa upgrade yerine bu kadar bekleyip sonraki wave başlar.")]
    [SerializeField] private float _autoNextWaveDelay = 3f;
    [SerializeField] private bool  _startOnAwake      = true;

    [Header("Roguelike Oda Sistemi")]
    [Tooltip("Bu odada kaç wave tamamlanınca RoomManager.OnCombatRoomComplete() çağrılır? 0 = sonsuz mod (klasik).")]
    [SerializeField] private int _maxWavesInRoom = 0;

    // ── Events ───────────────────────────────────────────────────────────
    /// <summary>Wave başlarken tetiklenir. int = wave numarası.</summary>
    public UnityEvent<int> OnWaveStart    = new UnityEvent<int>();
    /// <summary>Wave bitince tetiklenir. int = wave numarası.</summary>
    public UnityEvent<int> OnWaveComplete = new UnityEvent<int>();

    // ── State ────────────────────────────────────────────────────────────
    public int  CurrentWaveNumber    { get; private set; }
    public int  EnemiesAlive         { get; private set; }
    public bool WaveActive           { get; private set; }

    private int _wavesCompletedInRoom = 0;
    private readonly List<GameObject> _activeEnemies = new List<GameObject>();
    private float _eliteHealthMultiplier  = 1f;
    private float _eliteDamageMultiplier  = 1f;

    // ── Dahili Wave Tanımları ─────────────────────────────────────────────

    /// <summary>
    /// 5 dahili wave tanımı. _waveConfigs dizisi boşsa bunlar kullanılır.
    /// Her tanım: (zombiCount, timeBetweenSpawns, speedMul, damageMul, healthMul, useStrongPrefab)
    /// </summary>
    private static readonly (int count, float spawnInterval, float speed, float damage, float health, bool strong)[]
        BuiltInWaves =
    {
        //         sayı   aralık  hız    hasar   HP     güçlü?
        /* W1 */ (  8,    0.8f,   1.00f, 1.00f,  1.00f, false),
        /* W2 */ ( 12,    0.65f,  1.20f, 1.00f,  1.25f, false),
        /* W3 */ ( 16,    0.55f,  1.35f, 1.30f,  1.50f, true ),
        /* W4 */ ( 20,    0.45f,  1.50f, 1.50f,  1.80f, true ),
        /* W5 */ ( 25,    0.35f,  1.70f, 1.80f,  2.20f, true ),
    };

    // ── Lifecycle ─────────────────────────────────────────────────────────

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (GlobalGameState.IsWeaponUpgraded)
            FindFirstObjectByType<GunShooter>()?.SetNewGunEnabled(true);

        if (_startOnAwake)
            StartCoroutine(LaunchWave(Mathf.Max(1, GlobalGameState.SavedWave)));
    }

    void OnDestroy()
    {
        GameEvents.OnEnemyKilled -= HandleEnemyKilled;
    }

    // ── Wave Sequence ─────────────────────────────────────────────────────

    IEnumerator LaunchWave(int waveNumber)
    {
        CurrentWaveNumber         = waveNumber;
        GlobalGameState.SavedWave = waveNumber;

        _activeEnemies.Clear();
        EnemiesAlive = 0;
        WaveActive   = true;

        GameEvents.OnEnemyKilled += HandleEnemyKilled;

        _waveHUD?.ShowWaveAnnouncement(waveNumber);
        OnWaveStart?.Invoke(waveNumber);

        WaveConfig config = BuildConfig(waveNumber);
        EnemiesAlive      = CountEnemies(config);

        if (_spawner != null)
            yield return _spawner.SpawnWave(config, this);
        else
            Debug.LogError("[WaveManager] EnemySpawner referansı boş — Inspector'dan ata!");
    }

    void HandleEnemyKilled(GameObject enemy)
    {
        if (!WaveActive) return;

        EnemiesAlive = Mathf.Max(0, EnemiesAlive - 1);

        if (EnemiesAlive <= 0)
        {
            WaveActive = false;
            GameEvents.OnEnemyKilled -= HandleEnemyKilled;
            StartCoroutine(WaveCompleteRoutine());
        }
    }

    IEnumerator WaveCompleteRoutine()
    {
        _waveHUD?.ShowWaveComplete(CurrentWaveNumber);
        OnWaveComplete?.Invoke(CurrentWaveNumber);

        yield return new WaitForSecondsRealtime(1.5f);

        var  slotManager = UpgradeSlotManager.Instance;
        bool slotsFull   = slotManager != null && slotManager.AllSlotsFull();

        if (!slotsFull && _upgradeSelectionUI != null)
        {
            _upgradeSelectionUI.Show(CurrentWaveNumber);
            // UpgradeSelectionUI, kart seçince StartNextWave() çağırır.
        }
        else
        {
            yield return new WaitForSeconds(_autoNextWaveDelay);
            StartNextWave();
        }
    }

    // ── Public API ────────────────────────────────────────────────────────

    /// <summary>Upgrade seçildikten sonra UpgradeSelectionUI tarafından çağrılır.</summary>
    public void StartNextWave()
    {
        StartCoroutine(DelayedNextWave());
    }

    IEnumerator DelayedNextWave()
    {
        yield return new WaitForSeconds(1f);

        _wavesCompletedInRoom++;

        if (_maxWavesInRoom > 0 && _wavesCompletedInRoom >= _maxWavesInRoom)
        {
            // Roguelike: oda tamamlandı → sahne geçişi
            Debug.Log($"[WaveManager] Oda tamamlandı ({_wavesCompletedInRoom}/{_maxWavesInRoom} wave). RoomManager'a bildiriliyor.");
            RoomManager.Instance?.OnCombatRoomComplete();
        }
        else
        {
            StartCoroutine(LaunchWave(CurrentWaveNumber + 1));
        }
    }

    /// <summary>EnemySpawner tarafından her spawn sonrası çağrılır.</summary>
    public void RegisterEnemy(GameObject enemy)
    {
        _activeEnemies.Add(enemy);
    }

    /// <summary>RoomManager tarafından elite odalar için çağrılır.</summary>
    public void SetEliteMultipliers(float healthMul, float damageMul)
    {
        _eliteHealthMultiplier  = healthMul;
        _eliteDamageMultiplier  = damageMul;
    }

    // ── Config Builder ────────────────────────────────────────────────────

    WaveConfig BuildConfig(int waveNumber)
    {
        // 1. Inspector'dan atanmış WaveConfig varsa onu kullan
        int idx = waveNumber - 1;
        if (_waveConfigs != null && idx >= 0 && idx < _waveConfigs.Length && _waveConfigs[idx] != null)
            return _waveConfigs[idx];

        // 2. Dahili 5 wave tanımı (Wave 1-5)
        if (idx >= 0 && idx < BuiltInWaves.Length)
            return BuildFromBuiltIn(waveNumber, BuiltInWaves[idx]);

        // 3. Wave 6+ sonsuz mod
        return BuildInfiniteConfig(waveNumber);
    }

    WaveConfig BuildFromBuiltIn(int waveNumber,
        (int count, float spawnInterval, float speed, float damage, float health, bool strong) def)
    {
        GameObject prefab = (def.strong && _wave2EnemyPrefab != null) ? _wave2EnemyPrefab : _wave1EnemyPrefab;

        if (prefab == null)
        {
            Debug.LogError("[WaveManager] Prefab atanmamış! Inspector'dan _wave1EnemyPrefab'ı ata.");
            prefab = _wave1EnemyPrefab; // still null, EnemySpawner will skip
        }

        var config = ScriptableObject.CreateInstance<WaveConfig>();
        config.waveNumber        = waveNumber;
        config.timeBetweenSpawns = def.spawnInterval;
        config.speedMultiplier   = def.speed;
        config.damageMultiplier  = def.damage  * _eliteDamageMultiplier;
        config.healthMultiplier  = def.health  * _eliteHealthMultiplier;
        config.enemies.Add(new EnemySpawnEntry { enemyPrefab = prefab, count = def.count });
        return config;
    }

    WaveConfig BuildInfiniteConfig(int waveNumber)
    {
        // Wave 6+ → her wave 3 zombi artar, istatistikler de yükselir
        int   extraWaves = waveNumber - BuiltInWaves.Length - 1; // 0-based offset
        int   count      = 25 + extraWaves * _enemyCountIncreasePerWave;
        float speedMul   = 1.70f + extraWaves * 0.05f;
        float damageMul  = 1.80f + extraWaves * 0.10f;
        float healthMul  = 2.20f + extraWaves * 0.15f;

        GameObject prefab = _wave2EnemyPrefab != null ? _wave2EnemyPrefab : _wave1EnemyPrefab;

        var config = ScriptableObject.CreateInstance<WaveConfig>();
        config.waveNumber        = waveNumber;
        config.timeBetweenSpawns = Mathf.Max(0.2f, _timeBetweenSpawns - extraWaves * 0.02f);
        config.speedMultiplier   = speedMul;
        config.damageMultiplier  = damageMul * _eliteDamageMultiplier;
        config.healthMultiplier  = healthMul * _eliteHealthMultiplier;
        config.enemies.Add(new EnemySpawnEntry { enemyPrefab = prefab, count = count });
        return config;
    }

    int CountEnemies(WaveConfig config)
    {
        if (config == null) return 0;
        int total = 0;
        foreach (var e in config.enemies) total += e.count;
        return total;
    }
}
