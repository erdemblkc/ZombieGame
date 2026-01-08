using UnityEngine;

public class AntidotePickup : MonoBehaviour
{
    [Header("Interaction")]
    public KeyCode useKey = KeyCode.T;
    public float useRange = 2.0f;

    [Header("Consume")]
    public bool consumeOnUse = true;

    [Header("Refs (optional)")]
    public Transform player;
    private InfectionSystem _infection;

    void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (player != null)
            _infection = player.GetComponent<InfectionSystem>();
    }

    void Update()
    {
        if (player == null || _infection == null) return;

        float d = Vector3.Distance(player.position, transform.position);
        if (d > useRange) return;

        if (Input.GetKeyDown(useKey))
        {
            _infection.ResetInfection(); // ✅ direkt 0

            if (consumeOnUse)
                gameObject.SetActive(false);
        }
    }
}
