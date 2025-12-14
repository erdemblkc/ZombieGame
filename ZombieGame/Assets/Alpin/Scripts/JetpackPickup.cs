using UnityEngine;

public class JetpackPickup : MonoBehaviour
{
    [Header("Görsel Ayarlar")]
    public float rotateSpeed = 60f;   // sadece etrafýnda dönsün istiyorsan

    private void Update()
    {
        // Yerde duran jetpack'in hafif dönmesi için
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Sadece Player alsýn
        if (!other.CompareTag("Player"))
            return;

        // PlayerMovement scriptini bul (self, parent, child)
        PlayerMovement player = other.GetComponent<PlayerMovement>();
        if (player == null)
            player = other.GetComponentInParent<PlayerMovement>();
        if (player == null)
            player = other.GetComponentInChildren<PlayerMovement>();

        if (player != null)
        {
            player.SetHasJetpack(true);
            Debug.Log("Jetpack pickup: Jetpack alýndý, double jump + glide aktif.");
            Destroy(gameObject); // jetpack objesini sahneden sil
        }
        else
        {
            Debug.LogWarning("Jetpack pickup: PlayerMovement bulunamadý. Player objesindeki scripti kontrol et.");
        }
    }
}
