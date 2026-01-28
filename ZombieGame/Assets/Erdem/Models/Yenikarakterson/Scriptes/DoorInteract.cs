using UnityEngine;

public class DoorInteract : MonoBehaviour, IInteractable
{
    [Header("Door Rotation")]
    public Transform hinge; // DoorHinge buraya (yoksa transform)
    [Tooltip("Kapý kaç derece açýlsýn (pozitif býrak, yönü kod belirliyor)")]
    public float openAngle = 90f;
    public float speed = 6f;

    [Header("UI Prompt")]
    public string promptClosed = "E - Open Door";
    public string promptOpen = "E - Close Door";

    private bool isOpen;
    private Quaternion closedRot;
    private Quaternion openRot;

    // Interact eden player’ý hatýrlamak için (kapýyý hangi taraftan açacaðýz)
    private Transform lastInteractor;

    void Awake()
    {
        if (hinge == null) hinge = transform;

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
            float side = 1f;

            if (lastInteractor != null)
            {
                Vector3 toPlayer = (lastInteractor.position - hinge.position).normalized;
                side = Vector3.Dot(hinge.forward, toPlayer) >= 0f ? 1f : -1f;
            }

            openRot = closedRot * Quaternion.Euler(0f, openAngle * side, 0f);
            isOpen = true;
        }
        else
        {
            isOpen = false;
        }
    }
}
