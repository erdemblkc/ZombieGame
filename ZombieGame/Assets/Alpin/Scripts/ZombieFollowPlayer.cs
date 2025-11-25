using UnityEngine;

public class ZombieFollowPlayer : MonoBehaviour
{
    public float moveSpeed = 3f;     // Zombi hýz
    public float stopDistance = 1.5f; // Çok yakýna gelince duracaðý mesafe

    private Transform target;

    void Start()
    {
        // Sahnedeki Player tag'li objeyi bul
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            target = player.transform;
        }
        else
        {
            Debug.LogWarning("Player tag'li obje bulunamadý!");
        }
    }

    void Update()
    {
        if (target == null) return;

        // Hedefe doðru yön bul
        Vector3 direction = target.position - transform.position;
        direction.y = 0f; // Yukarý-aþaðý eðilmesin

        float distance = direction.magnitude;

        // Çok yaklaþmýþsa hareket etmesin
        if (distance <= stopDistance)
            return;

        direction.Normalize();

        // Ýleri doðru hareket
        transform.position += direction * moveSpeed * Time.deltaTime;

        // Hep oyuncuya doðru baksýn
        transform.forward = direction;
    }
}
