using UnityEngine;
using UnityEngine.AI;

public class ZombieSpawnOnKey : MonoBehaviour
{
    [Header("Spawn")]
    [SerializeField] private GameObject zombiePrefab;
    [SerializeField] private int spawnCount = 15;
    [SerializeField] private KeyCode spawnKey = KeyCode.H;

    [Header("Where")]
    [Tooltip("Spawn'larý bu merkezin etrafýnda arar. Boþ býrakýlýrsa bu GameObject'in pozisyonu kullanýlýr.")]
    [SerializeField] private Transform center;
    [SerializeField] private float searchRadius = 60f;

    [Header("Sampling")]
    [SerializeField] private int triesPerZombie = 30;
    [SerializeField] private float sampleMaxDistance = 4f;
    [SerializeField] private float minDistanceBetweenZombies = 1.2f;

    private readonly System.Collections.Generic.List<Vector3> spawnedPositions = new();

    private void Update()
    {
        if (Input.GetKeyDown(spawnKey))
            SpawnBatch();
    }

    private void SpawnBatch()
    {
        if (zombiePrefab == null)
        {
            Debug.LogError("ZombieSpawnOnKey: zombiePrefab boþ! Inspector'dan zombie prefabýný ata.");
            return;
        }

        spawnedPositions.Clear();

        Vector3 origin = center ? center.position : transform.position;

        int spawned = 0;

        for (int i = 0; i < spawnCount; i++)
        {
            if (TryGetRandomNavMeshPoint(origin, searchRadius, out Vector3 pos))
            {
                if (IsFarEnoughFromOthers(pos))
                {
                    Instantiate(zombiePrefab, pos, Quaternion.identity);
                    spawnedPositions.Add(pos);
                    spawned++;
                }
                else
                {
                    // Ayný yere yýðýlmasýn diye bir kez daha dene
                    i--;
                }
            }
            else
            {
                Debug.LogWarning($"ZombieSpawnOnKey: NavMesh üstünde nokta bulamadý. (spawned={spawned})");
                break;
            }
        }
    }

    private bool TryGetRandomNavMeshPoint(Vector3 origin, float radius, out Vector3 result)
    {
        for (int t = 0; t < triesPerZombie; t++)
        {
            Vector3 random = origin + Random.insideUnitSphere * radius;
            random.y = origin.y + 2f; // biraz yukarýdan sample almak daha stabil

            if (NavMesh.SamplePosition(random, out NavMeshHit hit, sampleMaxDistance, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }

        result = default;
        return false;
    }

    private bool IsFarEnoughFromOthers(Vector3 p)
    {
        float minSqr = minDistanceBetweenZombies * minDistanceBetweenZombies;
        for (int i = 0; i < spawnedPositions.Count; i++)
        {
            if ((spawnedPositions[i] - p).sqrMagnitude < minSqr)
                return false;
        }
        return true;
    }
}
