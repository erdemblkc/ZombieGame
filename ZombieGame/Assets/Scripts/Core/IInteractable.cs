using UnityEngine;

public interface IInteractable
{
    string GetPromptText();     // ekranda ne yazsýn?
    void Interact(GameObject interactor); // E’ye basýnca ne olacak?
}
