using UnityEngine;
using UnityEngine.AI;

public class ZombieSpawner1 : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject zombiePrefab;

    [Header("Spawn Area (Box)")]
    public Transform spawnArea;          // ZombieSpawnArea
    public Vector3 areaSize = new Vector3(50f, 1f, 50f); // XZ önemli

    [Header("Count")]
    public int spawnCount = 15;

    [Header("NavMesh")]
    public float navmeshSearchRadius = 8f;
    public int maxTriesPerZombie = 30;

    void Start()
    {
        SpawnWave();
    }

    [ContextMenu("Spawn Wave")]
    public void SpawnWave()
    {
        if (zombiePrefab == null)
        {
            Debug.LogError("ZombieSpawner1: zombiePrefab not assigned.");
            return;
        }
        if (spawnArea == null) spawnArea = transform;

        for (int i = 0; i < spawnCount; i++)
        {
            bool spawned = TrySpawnOne(i);
            if (!spawned)
                Debug.LogWarning($"ZombieSpawner1: Failed to spawn zombie #{i} (no navmesh point found).");
        }
    }

    bool TrySpawnOne(int index)
    {
        for (int t = 0; t < maxTriesPerZombie; t++)
        {
            Vector3 randomWorld = GetRandomPointInBox(spawnArea.position, areaSize);

            if (NavMesh.SamplePosition(randomWorld, out NavMeshHit hit, navmeshSearchRadius, NavMesh.AllAreas))
            {
                // Biraz yukarýdan koyup navmesh'e oturtuyoruz
                Vector3 pos = hit.position + Vector3.up * 0.02f;

                Instantiate(zombiePrefab, pos, Quaternion.identity);
                return true;
            }
        }
        return false;
    }

    Vector3 GetRandomPointInBox(Vector3 center, Vector3 size)
    {
        // Y'yi center'dan alýyoruz; asýl random XZ
        float x = Random.Range(-size.x * 0.5f, size.x * 0.5f);
        float z = Random.Range(-size.z * 0.5f, size.z * 0.5f);
        return new Vector3(center.x + x, center.y, center.z + z);
    }

    void OnDrawGizmosSelected()
    {
        if (spawnArea == null) spawnArea = transform;

        Gizmos.color = new Color(0f, 1f, 0f, 0.25f);
        Gizmos.DrawWireCube(spawnArea.position, areaSize);
    }
}
