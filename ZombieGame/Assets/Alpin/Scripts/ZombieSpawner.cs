using UnityEngine;

public class ZombieSpawner : MonoBehaviour
{
    public GameObject zombiePrefab;  // Buraya Zombie prefab'ýný sürükleyeceðiz
    public int zombieCount = 10;     // Kaç zombi spawnlansýn
    public float spawnRange = 20f;   // Spawner'ýn etrafýnda ne kadar alana yayýlsýnlar

    void Start()
    {
        SpawnZombies();
    }

    void SpawnZombies()
    {
        for (int i = 0; i < zombieCount; i++)
        {
            // Spawner'ýn etrafýnda rastgele bir x-z pozisyonu seç
            float randomX = Random.Range(-spawnRange, spawnRange);
            float randomZ = Random.Range(-spawnRange, spawnRange);

            Vector3 spawnPos = new Vector3(
                transform.position.x + randomX,
                transform.position.y,          // zeminin yüksekliðine göre ayarlayabilirsin
                transform.position.z + randomZ
            );

            Instantiate(zombiePrefab, spawnPos, Quaternion.identity);
        }
    }
}
