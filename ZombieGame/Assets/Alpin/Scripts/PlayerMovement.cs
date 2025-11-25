using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Hýz Ayarlarý")]
    public float walkSpeed = 5f;     // Normal yürüme hýzý
    public float runSpeed = 9f;      // Shift'e basýnca koþma hýzý

    [Header("Fizik")]
    public float gravity = -9.81f;   // Yerçekimi (negatif!)
    public float jumpHeight = 1.5f;  // Zýplama yüksekliði

    private CharacterController controller;
    private Vector3 velocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // Yere deðiyor muyuz?
        bool isGrounded = controller.isGrounded;

        // Yerdeyken aþaðý doðru hýz birikmesin
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // WASD input
        float horizontal = Input.GetAxis("Horizontal");  // A / D
        float vertical = Input.GetAxis("Vertical");    // W / S

        // Player'ýn baktýðý yöne göre hareket yönü
        Vector3 move = transform.right * horizontal + transform.forward * vertical;

        // Koþma: Left Shift basýlýysa runSpeed, deðilse walkSpeed
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        // Yatay hareket
        controller.Move(move * currentSpeed * Time.deltaTime);

        // Zýplama – sadece yere basýyorsak
        if (isGrounded && Input.GetButtonDown("Jump"))   // varsayýlan: Space
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Yerçekimi
        velocity.y += gravity * Time.deltaTime;

        // Düþme / zýplama hareketini uygula
        controller.Move(velocity * Time.deltaTime);
    }
}
