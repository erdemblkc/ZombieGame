using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombieSpawner1 : MonoBehaviour
{
    [Header("Prefab & Target")]
    public GameObject zombiePrefab;          // Wave 1
    public GameObject zombiePrefabWave2;     // Wave 2 (Zombie2 buraya)
    public Transform player;
   
    [Header("Weapon Mission")]
    public WeaponUpgradeManager weaponMission;

    [Header("Spawn Area (Plane Collider)")]
    public Collider spawnAreaCollider;

    [Header("Distance Rules")]
    public float minDistanceFromPlayer = 6f;

    [Header("NavMesh Sampling")]
    public float sampleMaxDistance = 8f;
    public int triesPerZombie = 40;

    [Header("Waves")]
    public int wave1Count = 10;
    public int wave2Count = 10;
    public float nextWaveDelay = 2f;

    // ✅ Wave2 buff IP TAL (istenildiği için artık kullanılmıyor)
    [Header("Wave 2 Buff (DISABLED)")]
    public float wave2SpeedMultiplier = 1.35f;
    public float wave2DamageMultiplier = 1.4f;
    public float wave2HealthMultiplier = 1.25f;

    // ✅ F2 ekstra hız IP TAL (istenildiği için artık kullanılmıyor)
    [Header("Manual F2 EXTRA Speed (DISABLED)")]
    public float manualF2ExtraSpeedMultiplier = 3.0f;

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

    void Awake()
    {
        var all = FindObjectsOfType<ZombieSpawner1>(true);
        Debug.Log($"[Spawner-Awake] Found ZombieSpawner1 count = {all.Length}. This = {gameObject.name} (id:{GetInstanceID()})", this);
        Debug.Log($"[Spawner-Awake] {gameObject.name} prefabWave1 = {(zombiePrefab ? zombiePrefab.name : "NULL")} prefabWave2 = {(zombiePrefabWave2 ? zombiePrefabWave2.name : "NULL")}", this);
    }

    void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        Debug.Log($"[Spawner-Start] {gameObject.name} (id:{GetInstanceID()}) prefabWave1={(zombiePrefab ? zombiePrefab.name : "NULL")} prefabWave2={(zombiePrefabWave2 ? zombiePrefabWave2.name : "NULL")} spawnArea={(spawnAreaCollider ? spawnAreaCollider.name : "NULL")} player={(player ? player.name : "NULL")}", this);

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
                    if (debugLogs) Debug.Log($"[DEBUG] Spawn Wave 2 (Zombie2) ({manualPressCount}/{manualPressLimit})", this);
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

            // ✅ manuel modda otomatik wave kontrolünü kapatıyoruz
            return;
        }

        // ---- Normal (otomatik) sistem ----
        if (currentWave == 0) return;

        if (Time.time < nextAliveCheckTime) return;
        nextAliveCheckTime = Time.time + aliveCheckInterval;

        for (int i = aliveZombies.Count - 1; i >= 0; i--)
        {
            if (aliveZombies[i] == null)
                aliveZombies.RemoveAt(i);
        }

        if (aliveZombies.Count == 0 && !waitingNextWave)
        {
            if (currentWave == 1)
            {
                waitingNextWave = true;
                StartCoroutine(SpawnNextWaveAfterDelay(2));
            }
        }
    }

    System.Collections.IEnumerator SpawnNextWaveAfterDelay(int waveNumber)
    {
        if (debugLogs) Debug.Log($"[Spawner] {gameObject.name} Wave {currentWave} cleared. Spawning Wave {waveNumber} in {nextWaveDelay}s...", this);
        yield return new WaitForSeconds(nextWaveDelay);

        SpawnWave(waveNumber);
        waitingNextWave = false;
    }

    void SpawnWave(int waveNumber)
    {
        if (zombiePrefab == null)
        {
            Debug.LogError($"ZombieSpawner: zombiePrefab (Wave1) atanmadı! -> {gameObject.name} (id:{GetInstanceID()})", this);
            return;
        }

        if (player == null)
        {
            Debug.LogError($"ZombieSpawner: player bulunamadı! -> {gameObject.name} (id:{GetInstanceID()})", this);
            return;
        }

        if (spawnAreaCollider == null)
        {
            Debug.LogError($"ZombieSpawner: spawnAreaCollider atanmadı! -> {gameObject.name} (id:{GetInstanceID()})", this);
            return;
        }

        currentWave = waveNumber;
        if (waveNumber == 1 && weaponMission != null)
            weaponMission.StartWeaponMission();

        aliveZombies.Clear();

        int count = (waveNumber == 1) ? wave1Count : wave2Count;

        // ✅ Wave 2 için farklı prefab seç
        GameObject prefabToUse = zombiePrefab;
        if (waveNumber == 2 && zombiePrefabWave2 != null)
            prefabToUse = zombiePrefabWave2;

        if (debugLogs) Debug.Log($"[Spawner] {gameObject.name} Spawning Wave {waveNumber} : {count} zombies | Prefab={prefabToUse.name}", this);

        for (int i = 0; i < count; i++)
        {
            Vector3 pos = FindSpawnPoint();
            GameObject z = Instantiate(prefabToUse, pos, Quaternion.identity);
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

            // ✅ Wave2 buff tamamen iptal edildi:
            // if (waveNumber == 2) ApplyWave2Buff(z);
        }
    }

    Vector3 FindSpawnPoint()
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
}
