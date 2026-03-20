using UnityEngine;

public class AntidotePickup : MonoBehaviour, IInteractable
{
    [Header("UI Prompt")]
    public string promptText = "E - Pick up Vaccine";

    [Header("Consume")]
    public bool consumeOnUse = true;

    public string GetPromptText()
    {
        return promptText;
    }

    public void Interact(GameObject interactor)
    {
        if (interactor == null) return;

        // Player'dan InfectionSystem bul
        InfectionSystem infection = interactor.GetComponentInChildren<InfectionSystem>();
        if (infection == null)
            infection = interactor.GetComponentInParent<InfectionSystem>();

        if (infection == null)
        {
            Debug.LogWarning("[AntidotePickup] InfectionSystem not found on interactor.");
            return;
        }

        infection.ResetInfection(); // ✅ direkt 0

        if (consumeOnUse)
            Destroy(gameObject);      // istersen SetActive(false) de yapabilirsin
    }
}
