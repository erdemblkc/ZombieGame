using UnityEngine;

public class GunPickup : MonoBehaviour
{
    public string playerTag = "Player";

    private bool playerInRange = false;
    private PlayerMovement playerMovement;

    void OnTriggerEnter(Collider other)
    {
        // Player trigger alanýna girdi mi?
        if (other.CompareTag(playerTag))
        {
            // PlayerMovement bileþenini player veya parentlerinde ara
            playerMovement = other.GetComponent<PlayerMovement>();
            if (playerMovement == null)
            {
                playerMovement = other.GetComponentInParent<PlayerMovement>();
            }

            if (playerMovement != null)
            {
                playerInRange = true;
                // Ýstersen burada ekrana "E'ye basarak silah al" UI'ý açabilirsin
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        // Player alandan çýktýysa
        if (other.CompareTag(playerTag))
        {
            playerInRange = false;
            playerMovement = null;
            // Ýstersen burada UI'ý kapat
        }
    }

    void Update()
    {
        if (playerInRange && playerMovement != null)
        {
            // E tuþuna basýldýðýnda silahý al
            if (Input.GetKeyDown(KeyCode.E))
            {
                // Player'ý silahlý moda al
                playerMovement.SetArmed(true);

                // Pickup objesini sahneden yok et
                gameObject.SetActive(false);
            }
        }
    }
}
