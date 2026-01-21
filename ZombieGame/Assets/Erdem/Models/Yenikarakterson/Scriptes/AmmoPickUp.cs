using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    public int ammoAmount = 16;                // kaç mermi versin
    public KeyCode pickupKey = KeyCode.E;      // E ile al
    public string playerTag = "Player";

    private GunShooter playerGun;              // player'ýn GunShooter'ý
    private bool playerInRange;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        playerInRange = true;

        // Player üzerinde veya child'larýnda GunShooter ara
        playerGun = other.GetComponentInChildren<GunShooter>();
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        playerInRange = false;
        playerGun = null;
    }

    void Update()
    {
        if (!playerInRange) return;
        if (playerGun == null) return;

        if (Input.GetKeyDown(pickupKey))
        {
            playerGun.AddReserveAmmo(ammoAmount);
            Destroy(gameObject);
        }
    }
}
