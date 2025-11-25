using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Hız Ayarları")]
    public float walkSpeed = 5f;     // Normal yürüme hızı
    public float runSpeed = 9f;      // Shift'e basınca koşma hızı

    [Header("Fizik")]
    public float gravity = -9.81f;   // Yerçekimi (negatif!)
    public float jumpHeight = 1.5f;  // Zıplama yüksekliği

    [Header("Animasyon")]
    public Animator animator;        // CharacterModel üzerindeki Animator'ı buraya ver
    public string speedParam = "Speed"; // Animator'daki float parametre adı

    private CharacterController controller;
    private Vector3 velocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // Yere değiyor muyuz?
        bool isGrounded = controller.isGrounded;

        // Yerdeyken aşağı doğru hız birikmesin
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // WASD input
        float horizontal = Input.GetAxis("Horizontal");  // A / D
        float vertical = Input.GetAxis("Vertical");      // W / S

        // Player'ın baktığı yöne göre hareket yönü
        Vector3 move = transform.right * horizontal + transform.forward * vertical;

        // Koşma: Left Shift basılıysa runSpeed, değilse walkSpeed
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        // Yatay hareket
        controller.Move(move * currentSpeed * Time.deltaTime);

        // ✅ Animasyon Speed (Idle/Walk geçişi için 0-1 arası)
        if (animator != null)
        {
            float speed01 = new Vector2(horizontal, vertical).magnitude; // 0=idle, 1=hareket
            animator.SetFloat(speedParam, speed01);
        }

        // Zıplama – sadece yere basıyorsak
        if (isGrounded && Input.GetButtonDown("Jump"))   // varsayılan: Space
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Yerçekimi
        velocity.y += gravity * Time.deltaTime;

        // Düşme / zıplama hareketini uygula
        controller.Move(velocity * Time.deltaTime);
    }
}
