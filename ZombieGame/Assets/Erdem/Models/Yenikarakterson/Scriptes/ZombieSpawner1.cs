using UnityEngine;
using UnityEngine.AI;

public class ZombieSpawner1 : MonoBehaviour
{
    [Header("Prefab & Target")]
    public GameObject zombiePrefab;      // ZombieRoot prefab
    public Transform player;

    [Header("Spawn Count")]
    public int count = 15;

    [Header("Spawn Area (Plane Collider)")]
    public Collider spawnAreaCollider;   // TestGround'un collider'ý (MeshCollider)

    [Header("Distance Rules")]
    public float minDistanceFromPlayer = 6f;

    [Header("NavMesh Sampling")]
    public float sampleMaxDistance = 8f; // Candidate noktanýn yakýnýnda navmesh arama mesafesi
    public int triesPerZombie = 40;

    void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        SpawnAll();
    }

    void SpawnAll()
    {
        if (zombiePrefab == null)
        {
            Debug.LogError("ZombieSpawner: zombiePrefab atanmadý!");
            return;
        }

        if (player == null)
        {
            Debug.LogError("ZombieSpawner: player bulunamadý! Player Tag=Player olmalý.");
            return;
        }

        if (spawnAreaCollider == null)
        {
            Debug.LogError("ZombieSpawner: spawnAreaCollider atanmadý! TestGround collider'ýný sürükle-býrak yap.");
            return;
        }

        for (int i = 0; i < count; i++)
        {
            Vector3 pos = FindSpawnPoint();

            GameObject z = Instantiate(zombiePrefab, pos, Quaternion.identity);

            // Zombiye player hedefini ver
            var ai = z.GetComponent<ZombieAI_Follow>();
            if (ai != null)
            {
                ai.target = player;
                ai.chaseRange = 999f; // testte kesin takip etsin
            }

            // Agent'ý navmesh üstüne garanti oturt (bazý durumlarda ilk framede lazým oluyor)
            var agent = z.GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                agent.Warp(pos);
            }
        }
    }

    Vector3 FindSpawnPoint()
    {
        Bounds b = spawnAreaCollider.bounds;

        for (int t = 0; t < triesPerZombie; t++)
        {
            // Plane'in her yerinden rastgele XZ seç
            float x = Random.Range(b.min.x, b.max.x);
            float z = Random.Range(b.min.z, b.max.z);

            // Yukarýdan örnekle (navmesh'e oturtacaðýz)
            Vector3 candidate = new Vector3(x, b.max.y + 2f, z);

            // player'a çok yakýn olmasýn (XZ)
            Vector3 flatPlayer = player.position; flatPlayer.y = 0f;
            Vector3 flatCand = candidate; flatCand.y = 0f;
            if (Vector3.Distance(flatCand, flatPlayer) < minDistanceFromPlayer)
                continue;

            // NavMesh üstüne oturt
            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, sampleMaxDistance, NavMesh.AllAreas))
            {
                return hit.position;
            }
        }

        // Olmazsa: collider merkezine yakýn bir yer
        NavMesh.SamplePosition(b.center, out NavMeshHit centerHit, 20f, NavMesh.AllAreas);
        return centerHit.position;
    }
}
