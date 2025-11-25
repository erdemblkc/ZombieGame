using UnityEngine;

public class FPSCameraController : MonoBehaviour
{
    public float sensitivity = 200f;   // Mouse hassasiyeti
    public Transform playerBody;       // Player kapsülü (vücut)

    float xRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; // Mouse'u ekrana kilitle
        Cursor.visible = false;                   // Ýmleci gizle
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;

        // Yukarý-aþaðý (kamera)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f); // Kafan arkaya dönmesin :)

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Saða-sola (vücudu döndür)
        playerBody.Rotate(Vector3.up * mouseX);
    }
}
