using UnityEngine;

public class ZombieWaveSpawner : MonoBehaviour
{
    [Header("Genel Ayarlar")]
    public GameObject zombiePrefab;         // Spawnlanacak zombi prefab'ı
    public int zombiesPerWave = 15;         // Her dalgada kaç zombi
    public float spawnRadius = 30f;         // Oyuncudan max uzaklık
    public float minDistanceFromPlayer = 10f; // Oyuncuya çok yakına spawn olmasın

    [Header("Zemin Raycast")]
    public LayerMask groundLayer;           // Yalnızca Ground layer'ına ray at

    [Header("Engeller")]
    public LayerMask blockedLayers;         // Binalar / duvarlar (spawn olmasın)

    [Header("Referanslar")]
    public Transform player;                // Player (boşsa otomatik bulur)

    void Start()
    {
        // Player referansı yoksa otomatik bul
        if (player == null)
        {
            PlayerHealth ph = FindFirstObjectByType<PlayerHealth>();
            if (ph != null)
            {
                player = ph.transform;
            }
        }
    }

    void Update()
    {
        // 🧪 Test: N tuşuna basınca bir wave spawnla
        if (Input.GetKeyDown(KeyCode.N))
        {
            SpawnWave();
        }
    }

    public void SpawnWave()
    {
        if (player == null || zombiePrefab == null)
        {
            Debug.LogWarning("ZombieWaveSpawner: Player veya Zombie Prefab eksik!");
            return;
        }

        for (int i = 0; i < zombiesPerWave; i++)
        {
            Vector3 spawnPos = GetRandomSpawnPosition();
            Instantiate(zombiePrefab, spawnPos, Quaternion.identity);
        }

        Debug.Log("Wave spawnlandı! Zombi sayısı: " + zombiesPerWave);
    }

    Vector3 GetRandomSpawnPosition()
    {
        Vector3 finalPos = player.position;
        int safety = 0;

        while (safety < 30) // uygun yer bulamazsa 30 deneme yapar
        {
            safety++;

            // Oyuncunun etrafında rastgele bir yön
            Vector2 circle = Random.insideUnitCircle.normalized;
            float distance = Random.Range(minDistanceFromPlayer, spawnRadius);

            Vector3 candidatePos = player.position +
                                   new Vector3(circle.x, 0f, circle.y) * distance;

            // Yukarıdan aşağı ray atıp zemini bul
            Vector3 rayStart = candidatePos + Vector3.up * 20f;
            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 50f, groundLayer))
            {
                Vector3 possiblePos = hit.point;

                // 🔴 Burada bina / engel var mı?
                float checkRadius = 1.0f;
                bool blocked = Physics.CheckSphere(
                    possiblePos + Vector3.up * 0.5f,
                    checkRadius,
                    blockedLayers
                );

                if (blocked)
                {
                    // Bu pozisyon bina içinde / dibinde → yeniden dene
                    continue;
                }

                finalPos = possiblePos;
                break;
            }
            else
            {
                // Zemin bulunamazsa fallback
                finalPos = new Vector3(candidatePos.x, player.position.y, candidatePos.z);
            }
        }

        return finalPos;
    }

    // Editörde menzili görmek için
    private void OnDrawGizmosSelected()
    {
        if (player == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(player.position, minDistanceFromPlayer);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(player.position, spawnRadius);
    }
}
