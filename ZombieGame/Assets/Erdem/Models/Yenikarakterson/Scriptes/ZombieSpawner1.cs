using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombieSpawner1 : MonoBehaviour
{
    [Header("Prefab & Target")]
    public GameObject zombiePrefab;          // Wave 1 (ZombieRoot prefab)
    public GameObject zombiePrefabWave2;     // Wave 2 (ZombieRoot2 prefab)
    public Transform player;
    [Header("Wave 2 Buff (Root2)")]
    public float wave2SpeedMultiplier = 1.35f;
    public float wave2DamageMultiplier = 1.6f;
    public float wave2HealthMultiplier = 1.5f;  // %50 daha fazla can


    [Header("Weapon Mission (Optional)")]
    public WeaponUpgradeManager weaponMission;

    [Header("Wave Complete UI (Optional)")]
    public WaveCompleteManager waveUI; // Wave 1 bitince panel açacak

    [Header("Spawn Area (Optional Plane Collider)")]
    public Collider spawnAreaCollider; // İstersen ver, yoksa radius ile spawn eder

    [Header("Spawn Area (Fallback: Radius Around Spawner)")]
    public float spawnRadius = 25f;   // spawnAreaCollider yoksa bununla spawn eder

    [Header("Distance Rules")]
    public float minDistanceFromPlayer = 6f;

    [Header("NavMesh Sampling")]
    public float sampleMaxDistance = 8f;
    public int triesPerZombie = 40;

    [Header("Waves")]
    public int wave1Count = 10;
    public int wave2Count = 10;
    public float nextWaveDelay = 2f;

    [Header("Alive Check")]
    public float aliveCheckInterval = 0.5f;

    [Header("Debug")]
    public bool debugLogs = true;

    [Header("Manual Debug Spawn (Demo)")]
    public bool manualDebugMode = true;
    public bool autoSpawnWave1OnStart = false;
    public KeyCode spawnWave1Key = KeyCode.F1;
    public KeyCode spawnWave2Key = KeyCode.F2;
    public KeyCode spawnNextWaveKey = KeyCode.F3;
    public int manualPressLimit = 2;

    private readonly List<GameObject> aliveZombies = new List<GameObject>();
    private int currentWave = 0;
    private float nextAliveCheckTime = 0f;
    private bool waitingNextWave = false;

    private int manualPressCount = 0;

    void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (debugLogs)
        {
            Debug.Log($"[Spawner-Start] {gameObject.name} prefabWave1={(zombiePrefab ? zombiePrefab.name : "NULL")} " +
                      $"prefabWave2={(zombiePrefabWave2 ? zombiePrefabWave2.name : "NULL")} " +
                      $"spawnArea={(spawnAreaCollider ? spawnAreaCollider.name : "NULL")} " +
                      $"player={(player ? player.name : "NULL")} " +
                      $"waveUI={(waveUI ? waveUI.name : "NULL")}", this);
        }

        if (autoSpawnWave1OnStart)
        {
            SpawnWave(1);
        }
    }

    void Update()
    {
        // ✅ MANUAL DEBUG MODE: tuşlarla spawn
        if (manualDebugMode)
        {
            if (manualPressCount < manualPressLimit)
            {
                if (Input.GetKeyDown(spawnWave1Key))
                {
                    manualPressCount++;
                    SpawnWave(1);
                    if (debugLogs) Debug.Log($"[DEBUG] Spawn Wave 1 ({manualPressCount}/{manualPressLimit})", this);
                }
                else if (Input.GetKeyDown(spawnWave2Key))
                {
                    manualPressCount++;
                    SpawnWave(2);
                    if (debugLogs) Debug.Log($"[DEBUG] Spawn Wave 2 ({manualPressCount}/{manualPressLimit})", this);
                }
                else if (Input.GetKeyDown(spawnNextWaveKey))
                {
                    manualPressCount++;
                    int next = (currentWave <= 0) ? 1 : currentWave + 1;
                    if (next > 2) next = 2;
                    SpawnWave(next);
                    if (debugLogs) Debug.Log($"[DEBUG] Spawn NEXT Wave -> {next} ({manualPressCount}/{manualPressLimit})", this);
                }
            }

            // manual modda otomatik wave kontrolü yok
            return;
        }

        // ---- Normal (otomatik) sistem ----
        if (currentWave == 0) return;

        if (Time.time < nextAliveCheckTime) return;
        nextAliveCheckTime = Time.time + aliveCheckInterval;

        // null olanları temizle
        for (int i = aliveZombies.Count - 1; i >= 0; i--)
        {
            if (aliveZombies[i] == null)
                aliveZombies.RemoveAt(i);
        }

        // Wave temizlendiyse
        if (aliveZombies.Count == 0 && !waitingNextWave)
        {
            if (currentWave == 1)
            {
                // Wave 1 bitti -> panel aç (oyunu durdur)
                waitingNextWave = true;

                if (waveUI != null)
                {
                    if (debugLogs) Debug.Log("[Spawner] Wave 1 cleared -> showing upgrade panel.", this);
                    waveUI.ShowPanel();
                    // Panel kapatılınca UI tarafı SpawnNextWaveFromUI() çağıracak
                }
                else
                {
                    // UI yoksa eski davranış: gecikmeyle wave2
                    StartCoroutine(SpawnNextWaveAfterDelay(2));
                }
            }
        }
    }

    IEnumerator SpawnNextWaveAfterDelay(int waveNumber)
    {
        if (debugLogs) Debug.Log($"[Spawner] Wave {currentWave} cleared. Spawning Wave {waveNumber} in {nextWaveDelay}s...", this);
        yield return new WaitForSeconds(nextWaveDelay);

        SpawnWave(waveNumber);
        waitingNextWave = false;
    }

    // ✅ UI butonu seçilince WaveCompleteManager burayı çağıracak
    public void SpawnNextWaveFromUI()
    {
        if (debugLogs) Debug.Log("[Spawner] SpawnNextWaveFromUI called -> spawning Wave 2", this);
        SpawnWave(2);
        waitingNextWave = false;
    }

    void SpawnWave(int waveNumber)
    {
        if (zombiePrefab == null)
        {
            Debug.LogError($"ZombieSpawner: zombiePrefab (Wave1) atanmadı! -> {gameObject.name}", this);
            return;
        }

        if (player == null)
        {
            Debug.LogError($"ZombieSpawner: player bulunamadı! -> {gameObject.name}", this);
            return;
        }

        currentWave = waveNumber;

        if (waveNumber == 1 && weaponMission != null)
            weaponMission.StartWeaponMission();

        aliveZombies.Clear();

        int count = (waveNumber == 1) ? wave1Count : wave2Count;

        GameObject prefabToUse = zombiePrefab;
        if (waveNumber == 2 && zombiePrefabWave2 != null)
            prefabToUse = zombiePrefabWave2;

        if (debugLogs)
            Debug.Log($"[Spawner] Spawning Wave {waveNumber} : {count} zombies | Prefab={prefabToUse.name} | SpawnMode={(spawnAreaCollider ? "ColliderBounds" : "RadiusAroundSpawner")}", this);

        for (int i = 0; i < count; i++)
        {
            Vector3 pos = FindSpawnPoint();
            GameObject z = Instantiate(prefabToUse, pos, Quaternion.identity);
            if(waveNumber == 2 && prefabToUse == zombiePrefabWave2)
{
                var agent2 = z.GetComponent<UnityEngine.AI.NavMeshAgent>();
                if (agent2 != null)
                    agent2.speed *= wave2SpeedMultiplier;

                var atk = z.GetComponentInChildren<ZombieAttackDamageTimed>();
                if (atk != null)
                    atk.damage *= wave2DamageMultiplier;

                var hp = z.GetComponent<ZombieHealth1>();
                if (hp != null)
                {
                    hp.maxHealth *= wave2HealthMultiplier;
                    hp.currentHealth = hp.maxHealth;
                }
            }


            aliveZombies.Add(z);

            var ai = z.GetComponent<ZombieAI_Follow>();
            if (ai != null)
            {
                ai.target = player;
                ai.chaseRange = 999f;
            }

            var agent = z.GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                agent.Warp(pos);
                agent.velocity = Vector3.zero;
                agent.ResetPath();
            }
        }
    }

    Vector3 FindSpawnPoint()
    {
        // 1) Eğer spawnAreaCollider varsa collider bounds içinde dene
        if (spawnAreaCollider != null)
        {
            Bounds b = spawnAreaCollider.bounds;

            for (int t = 0; t < triesPerZombie; t++)
            {
                float x = Random.Range(b.min.x, b.max.x);
                float z = Random.Range(b.min.z, b.max.z);

                Vector3 candidate = new Vector3(x, b.max.y + 2f, z);

                Vector3 flatPlayer = player.position; flatPlayer.y = 0f;
                Vector3 flatCand = candidate; flatCand.y = 0f;
                if (Vector3.Distance(flatCand, flatPlayer) < minDistanceFromPlayer)
                    continue;

                if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, sampleMaxDistance, NavMesh.AllAreas))
                    return hit.position;
            }

            NavMesh.SamplePosition(b.center, out NavMeshHit centerHit, 20f, NavMesh.AllAreas);
            return centerHit.position;
        }

        // 2) Collider yoksa spawner etrafında radius ile dene
        Vector3 center = transform.position;

        for (int t = 0; t < triesPerZombie; t++)
        {
            Vector3 random = center + new Vector3(
                Random.Range(-spawnRadius, spawnRadius),
                0f,
                Random.Range(-spawnRadius, spawnRadius)
            );

            Vector3 flatPlayer = player.position; flatPlayer.y = 0f;
            Vector3 flatRnd = random; flatRnd.y = 0f;
            if (Vector3.Distance(flatRnd, flatPlayer) < minDistanceFromPlayer)
                continue;

            if (NavMesh.SamplePosition(random, out NavMeshHit hit, sampleMaxDistance, NavMesh.AllAreas))
                return hit.position;
        }

        NavMesh.SamplePosition(center, out NavMeshHit fallbackHit, 50f, NavMesh.AllAreas);
        return fallbackHit.position;
    }
}
