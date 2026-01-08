using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombieSpawner1 : MonoBehaviour
{
    [Header("Prefab & Target")]
    public GameObject zombiePrefab;
    public Transform player;

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

    [Header("Wave 2 Buff")]
    public float wave2SpeedMultiplier = 1.35f;
    public float wave2DamageMultiplier = 1.4f;
    public float wave2HealthMultiplier = 1.25f;

    [Header("Alive Check")]
    public float aliveCheckInterval = 0.5f;

    [Header("Debug")]
    public bool debugLogs = true;

    private readonly List<GameObject> aliveZombies = new List<GameObject>();
    private int currentWave = 0;
    private float nextAliveCheckTime = 0f;
    private bool waitingNextWave = false;

    void Awake()
    {
        // ✅ Sahnedeki tüm spawner'ları say (disable olanlar dahil)
        var all = FindObjectsOfType<ZombieSpawner1>(true);
        Debug.Log($"[Spawner-Awake] Found ZombieSpawner1 count = {all.Length}. This = {gameObject.name} (id:{GetInstanceID()})", this);

        // ✅ Prefab referansını daha baştan yazdır
        Debug.Log($"[Spawner-Awake] {gameObject.name} prefab = {(zombiePrefab ? zombiePrefab.name : "NULL")}", this);
    }

    void Start()
    {
        // player otomatik bul
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        Debug.Log($"[Spawner-Start] {gameObject.name} (id:{GetInstanceID()}) prefab={(zombiePrefab ? zombiePrefab.name : "NULL")} spawnArea={(spawnAreaCollider ? spawnAreaCollider.name : "NULL")} player={(player ? player.name : "NULL")}", this);

        SpawnWave(1);
    }

    void Update()
    {
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
        // ✅ Hata mesajına objenin adını + id'yi ekledim
        if (zombiePrefab == null)
        {
            Debug.LogError($"ZombieSpawner: zombiePrefab atanmadı! -> {gameObject.name} (id:{GetInstanceID()})", this);
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
        aliveZombies.Clear();

        int count = (waveNumber == 1) ? wave1Count : wave2Count;

        if (debugLogs) Debug.Log($"[Spawner] {gameObject.name} Spawning Wave {waveNumber} : {count} zombies", this);

        for (int i = 0; i < count; i++)
        {
            Vector3 pos = FindSpawnPoint();
            GameObject z = Instantiate(zombiePrefab, pos, Quaternion.identity);
            aliveZombies.Add(z);

            var ai = z.GetComponent<ZombieAI_Follow>();
            if (ai != null)
            {
                ai.target = player;
                ai.chaseRange = 999f;
            }

            var agent = z.GetComponent<NavMeshAgent>();
            if (agent != null) agent.Warp(pos);

            if (waveNumber == 2)
                ApplyWave2Buff(z);
        }
    }

    void ApplyWave2Buff(GameObject z)
    {
        var agent = z.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.speed *= wave2SpeedMultiplier;
            agent.acceleration *= wave2SpeedMultiplier;
        }

        var atk = z.GetComponent<ZombieAttackDamageTimed>();
        if (atk != null)
        {
            atk.damage *= wave2DamageMultiplier;
        }

        var hp = z.GetComponent<ZombieHealth1>();
        if (hp != null)
        {
            hp.maxHealth *= wave2HealthMultiplier;
            hp.currentHealth = hp.maxHealth;
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
