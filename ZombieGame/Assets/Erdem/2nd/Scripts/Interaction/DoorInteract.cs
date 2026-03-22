using UnityEngine;

[RequireComponent(typeof(AudioSource))] // Otomatik AudioSource ekler
public class DoorInteract : MonoBehaviour, IInteractable
{
    [Header("Door Rotation")]
    public Transform hinge; // DoorHinge buraya (yoksa transform)
    [Tooltip("Kapý kaç derece açýlsýn (pozitif býrak, yönü kod belirliyor)")]
    public float openAngle = 90f;
    public float speed = 6f;

    [Header("Audio (YENÝ)")]
    public AudioSource audioSource;
    public AudioClip openSound;  // Gýcýrtý sesi
    public AudioClip closeSound; // Kapanma sesi

    [Header("UI Prompt")]
    public string promptClosed = "E - Open Door";
    public string promptOpen = "E - Close Door";

    private bool isOpen;
    private Quaternion closedRot;
    private Quaternion openRot;

    // Interact eden player’ý hatýrlamak için
    private Transform lastInteractor;

    void Awake()
    {
        if (hinge == null) hinge = transform;

        // Otomatik AudioSource bulma
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        closedRot = hinge.localRotation;
        openRot = closedRot;
    }

    void Update()
    {
        Quaternion target = isOpen ? openRot : closedRot;
        hinge.localRotation = Quaternion.Slerp(hinge.localRotation, target, Time.deltaTime * speed);

        if (Quaternion.Angle(hinge.localRotation, target) < 0.5f)
            hinge.localRotation = target;
    }

    public string GetPromptText()
    {
        return isOpen ? promptOpen : promptClosed;
    }

    public void Interact(GameObject interactor)
    {
        if (interactor != null)
            lastInteractor = interactor.transform;

        if (!isOpen)
        {
            // --- KAPIYI AÇ ---
            float side = 1f;

            if (lastInteractor != null)
            {
                Vector3 toPlayer = (lastInteractor.position - hinge.position).normalized;
                side = Vector3.Dot(hinge.forward, toPlayer) >= 0f ? 1f : -1f;
            }

            openRot = closedRot * Quaternion.Euler(0f, openAngle * side, 0f);
            isOpen = true;

            // Ses Çal
            PlaySound(openSound);
        }
        else
        {
            // --- KAPIYI KAPAT ---
            isOpen = false;

            // Ses Çal
            PlaySound(closeSound);
        }
    }

    // Ses çalma yardýmcýsý (Pitch varyasyonu ile)
    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f); // Hafif ton farký (doðallýk için)
            audioSource.PlayOneShot(clip);
        }
    }
}