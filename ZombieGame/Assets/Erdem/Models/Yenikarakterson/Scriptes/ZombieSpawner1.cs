using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombieSpawner1 : MonoBehaviour
{
    [Header("Prefab & Target")]
    public GameObject zombiePrefab;          // Wave 1
    public GameObject zombiePrefabWave2;     // Wave 2
    public Transform player;

    [Header("Wave 2 Buff")]
    public float wave2SpeedMultiplier = 1.35f;
    public float wave2DamageMultiplier = 1.6f;
    public float wave2HealthMultiplier = 1.5f;

    [Header("Weapon Mission (Optional)")]
    public WeaponUpgradeManager weaponMission;

    [Header("Wave Complete UI (Optional)")]
    public WaveCompleteManager waveUI;

    [Header("Spawn Area")]
    public Collider spawnAreaCollider;
    public float spawnRadius = 25f;

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
    public bool autoSpawnWave1OnStart = true; // Start'ta otomatik başlasın mı?

    private readonly List<GameObject> aliveZombies = new List<GameObject>();
    private int currentWave = 0;
    private float nextAliveCheckTime = 0f;
    private bool waitingNextWave = false;

    void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        // --- HAFIZA KONTROLÜ (BURASI EKLENDİ) ---
        // Eğer oyun yeniden başladıysa ve kayıtlı wave 1'den büyükse (yani 2 ise)
        if (GlobalGameState.SavedWave > 1)
        {
            // Otomatik spawn'ı kapat ki üst üste binmesin
            autoSpawnWave1OnStart = false;

            // Kayıtlı wave'i başlat
            SpawnWave(GlobalGameState.SavedWave);
        }
        // ----------------------------------------

        if (autoSpawnWave1OnStart)
        {
            SpawnWave(1);
        }
    }

    void Update()
    {
        if (currentWave == 0) return;

        if (Time.time < nextAliveCheckTime) return;
        nextAliveCheckTime = Time.time + aliveCheckInterval;

        // null olanları temizle (ölen zombiler)
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
                // Wave 1 bitti -> panel aç
                waitingNextWave = true;

                if (waveUI != null)
                {
                    waveUI.ShowPanel();
                }
                else
                {
                    StartCoroutine(SpawnNextWaveAfterDelay(2));
                }
            }
        }
    }

    IEnumerator SpawnNextWaveAfterDelay(int waveNumber)
    {
        yield return new WaitForSeconds(nextWaveDelay);
        SpawnWave(waveNumber);
        waitingNextWave = false;
    }

    // UI butonu seçilince burası çağrılır
    public void SpawnNextWaveFromUI()
    {
        SpawnWave(2);
        waitingNextWave = false;
    }

    // --- İŞTE BU FONKSİYON EKSİKTİ VEYA HATALIYDI ---
    void SpawnWave(int waveNumber)
    {
        if (zombiePrefab == null || player == null) return;

        currentWave = waveNumber;

        // --- HAFIZAYA KAYDET (BURASI EKLENDİ) ---
        GlobalGameState.SavedWave = currentWave;
        // ----------------------------------------

        if (waveNumber == 1 && weaponMission != null)
            weaponMission.StartWeaponMission();

        aliveZombies.Clear();

        int count = (waveNumber == 1) ? wave1Count : wave2Count;
        GameObject prefabToUse = (waveNumber == 2 && zombiePrefabWave2 != null) ? zombiePrefabWave2 : zombiePrefab;

        for (int i = 0; i < count; i++)
        {
            Vector3 pos = FindSpawnPoint();
            GameObject z = Instantiate(prefabToUse, pos, Quaternion.identity);

            // Wave 2 Güçlendirmeleri
            if (waveNumber == 2)
            {
                var agent2 = z.GetComponent<NavMeshAgent>();
                if (agent2 != null) agent2.speed *= wave2SpeedMultiplier;

                var atk = z.GetComponentInChildren<ZombieAttackDamageTimed>();
                if (atk != null) atk.damage *= wave2DamageMultiplier;

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
                ai.chaseRange = 999f; // Kaçamasın
            }

            var agent = z.GetComponent<NavMeshAgent>();
            if (agent != null) agent.Warp(pos);
        }
    }

    Vector3 FindSpawnPoint()
    {
        // 1) Collider varsa içinde
        if (spawnAreaCollider != null)
        {
            Bounds b = spawnAreaCollider.bounds;
            for (int t = 0; t < triesPerZombie; t++)
            {
                float x = Random.Range(b.min.x, b.max.x);
                float z = Random.Range(b.min.z, b.max.z);
                Vector3 candidate = new Vector3(x, b.max.y + 2f, z);

                if (Vector3.Distance(candidate, player.position) < minDistanceFromPlayer) continue;

                if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, sampleMaxDistance, NavMesh.AllAreas))
                    return hit.position;
            }
            return transform.position; // Fallback
        }

        // 2) Yoksa radius etrafında
        Vector3 center = transform.position;
        for (int t = 0; t < triesPerZombie; t++)
        {
            Vector3 random = center + new Vector3(Random.Range(-spawnRadius, spawnRadius), 0f, Random.Range(-spawnRadius, spawnRadius));
            if (Vector3.Distance(random, player.position) < minDistanceFromPlayer) continue;

            if (NavMesh.SamplePosition(random, out NavMeshHit hit, sampleMaxDistance, NavMesh.AllAreas))
                return hit.position;
        }
        return transform.position;
    }
}