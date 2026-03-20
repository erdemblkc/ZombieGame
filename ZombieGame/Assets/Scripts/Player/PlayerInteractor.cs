using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    public KeyCode interactKey = KeyCode.E;

    // ✅ Interface değil, Unity Object tutuyoruz (destroy olunca null olur)
    private readonly List<MonoBehaviour> inRange = new List<MonoBehaviour>();
    private MonoBehaviour currentMb;

    void Update()
    {
        currentMb = GetBestInteractableMB();

        if (currentMb != null)
        {
            var interactable = currentMb as IInteractable;

            // Destroy sonrası güvenlik
            if (interactable == null)
            {
                RemoveDeadEntries();
                InteractPromptUI.Instance?.Hide();
                return;
            }

            InteractPromptUI.Instance?.Show(interactable.GetPromptText());

            if (Input.GetKeyDown(interactKey))
            {
                interactable.Interact(gameObject);

                // ✅ Interact sonrası (Destroy olabilir) hemen temizle
                RemoveDeadEntries();
                currentMb = null;

                // Tekrar seçim yap
                var next = GetBestInteractableMB();
                if (next == null) InteractPromptUI.Instance?.Hide();
            }
        }
        else
        {
            InteractPromptUI.Instance?.Hide();
        }
    }

    MonoBehaviour GetBestInteractableMB()
    {
        RemoveDeadEntries();
        if (inRange.Count == 0) return null;
        return inRange[inRange.Count - 1]; // en son girilen
    }

    void RemoveDeadEntries()
    {
        for (int i = inRange.Count - 1; i >= 0; i--)
        {
            if (inRange[i] == null) // ✅ Unity destroy check
                inRange.RemoveAt(i);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Parent chain’de IInteractable implement eden ilk MonoBehaviour’u bul
        var mbs = other.GetComponentsInParent<MonoBehaviour>();
        for (int i = 0; i < mbs.Length; i++)
        {
            if (mbs[i] is IInteractable)
            {
                if (!inRange.Contains(mbs[i]))
                    inRange.Add(mbs[i]);
                return;
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        var mbs = other.GetComponentsInParent<MonoBehaviour>();
        for (int i = 0; i < mbs.Length; i++)
        {
            if (mbs[i] is IInteractable)
            {
                inRange.Remove(mbs[i]);
                return;
            }
        }
    }
}
