using UnityEngine;
using UnityEngine.AI;

public class DemoWaveSpawnerB : MonoBehaviour
{
    [Header("Prefab & Target")]
    public GameObject zombiePrefab;     // ZombieRoot prefab (Project'ten ata!)
    public Transform player;            // boş bırakabilirsin, Tag=Player ile bulur

    [Header("Spawn Area")]
    public Collider spawnAreaCollider;  // TestGround collider

    [Header("Spawn Settings")]
    public KeyCode spawnKey = KeyCode.B;
    public int spawnCount = 10;
    public float minDistanceFromPlayer = 6f;

    [Header("NavMesh Sampling")]
    public float sampleMaxDistance = 8f;
    public int triesPerZombie = 40;

    [Header("Buff (B ile gelenler)")]
    public float speedMultiplier = 1.35f;
    public float damageMultiplier = 1.4f;
    public float healthMultiplier = 1.25f;

    [Header("Fix Backwards Visual")]
    [Tooltip("B ile spawn olanların sırt dönük yürümesini düzeltmek için. Çoğu durumda 0 doğru, olmazsa 180 dene.")]
    public float spawnedVisualYawOffset = 0f;

    [Tooltip("Ekstra garanti: spawn olur olmaz visual'ı local Y rotasyonuna zorla uygula.")]
    public bool forceVisualLocalYawOnSpawn = true;

    [Header("Debug")]
    public bool debugLogs = true;

    void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (debugLogs)
            Debug.Log($"[DemoWaveSpawnerB] Ready. Press '{spawnKey}' to spawn {spawnCount} buffed zombies.", this);
    }

    void Update()
    {
        if (Input.GetKeyDown(spawnKey))
        {
            SpawnBuffedBatch();
        }
    }

    void SpawnBuffedBatch()
    {
        if (zombiePrefab == null)
        {
            Debug.LogError("[DemoWaveSpawnerB] zombiePrefab is NULL! Project'ten ZombieRoot prefabını ata.", this);
            return;
        }

        if (spawnAreaCollider == null)
        {
            Debug.LogError("[DemoWaveSpawnerB] spawnAreaCollider is NULL! TestGround collider'ını ata.", this);
            return;
        }

        // Player yoksa tekrar dene
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (player == null)
        {
            Debug.LogError("[DemoWaveSpawnerB] Player bulunamadı! Player objesinin Tag'i 'Player' olmalı.", this);
            return;
        }

        if (debugLogs) Debug.Log($"[DemoWaveSpawnerB] Spawning {spawnCount} buffed zombies...", this);

        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 pos = FindSpawnPoint(player.position);

            GameObject z = Instantiate(zombiePrefab, pos, Quaternion.identity);

            // AI target ver (takip etsin)
            var ai = z.GetComponent<ZombieAI_Follow>();
            if (ai != null)
            {
                ai.target = player;
                ai.chaseRange = 999f;

                // ✅ SUNUM FIX: sonradan spawn olanların ters yürümesini düzelt
                ai.visualYawOffset = spawnedVisualYawOffset;

                // Ekstra garanti: visual'ı anında aynı yöne kilitle
                if (forceVisualLocalYawOnSpawn && ai.visual != null)
                {
                    Vector3 e = ai.visual.localEulerAngles;
                    e.y = spawnedVisualYawOffset;
                    ai.visual.localEulerAngles = e;
                }
            }

            // NavMesh üstüne oturt
            var agent = z.GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                agent.Warp(pos);

                // hız buff
                agent.speed *= speedMultiplier;
                agent.acceleration *= speedMultiplier;
            }

            // damage buff
            var atk = z.GetComponent<ZombieAttackDamageTimed>();
            if (atk != null)
                atk.damage *= damageMultiplier;

            // health buff
            var hp = z.GetComponent<ZombieHealth1>();
            if (hp != null)
            {
                hp.maxHealth *= healthMultiplier;
                hp.currentHealth = hp.maxHealth;
            }
        }
    }

    Vector3 FindSpawnPoint(Vector3 playerPos)
    {
        Bounds b = spawnAreaCollider.bounds;

        for (int t = 0; t < triesPerZombie; t++)
        {
            float x = Random.Range(b.min.x, b.max.x);
            float z = Random.Range(b.min.z, b.max.z);

            Vector3 candidate = new Vector3(x, b.max.y + 2f, z);

            Vector3 flatPlayer = playerPos; flatPlayer.y = 0f;
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
