using UnityEngine;

public class AmmoPickup : MonoBehaviour, IInteractable
{
    [Header("Pickup Settings")]
    public int ammoAmount = 16;

    [Header("UI Prompt")]
    public string promptText = "E - Pick up Ammo";

    public string GetPromptText()
    {
        return promptText;
    }

    public void Interact(GameObject interactor)
    {
        if (interactor == null) return;

        // Player üzerindeki / child'ýndaki GunShooter'ý bul
        GunShooter gun = interactor.GetComponentInChildren<GunShooter>();
        if (gun == null)
            gun = interactor.GetComponentInParent<GunShooter>();

        if (gun == null)
        {
            Debug.LogWarning("[AmmoPickup] GunShooter not found on interactor.");
            return;
        }

        gun.AddReserveAmmo(ammoAmount);
        Destroy(gameObject);
    }
}
