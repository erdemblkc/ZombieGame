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

    [Header("Knockback")]
    public float knockbackDamp = 3f; // Geri tepme ne kadar hızlı sönsün

    private CharacterController controller;
    private Vector3 velocity;        // Zıplama / düşme için dikey hız
    private Vector3 impact;          // Knockback vektörü

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

        // 🔹 Knockback vektörünü yavaşça sıfıra doğru azalt
        if (impact.magnitude > 0.2f)
        {
            impact = Vector3.Lerp(impact, Vector3.zero, knockbackDamp * Time.deltaTime);
        }
        else
        {
            impact = Vector3.zero;
        }

        // 🧪 TEST: K tuşuna basınca oyuncuyu geriye doğru it
        if (Input.GetKeyDown(KeyCode.K))
        {
            // -transform.forward = yüzümüzün tam tersi yön (tam geriye)
            AddImpact(-transform.forward, 18f, 1f);
        }

        // Tüm hareketleri tek Move çağrısında birleştir
        Vector3 finalMove =
            (move * currentSpeed) +          // normal hareket
            impact +                         // knockback
            new Vector3(0f, velocity.y, 0f); // zıplama/düşme

        controller.Move(finalMove * Time.deltaTime);
    }

    /// <summary>
    /// Dışarıdan knockback uygulamak için (zombi vs.)
    /// </summary>
    public void AddImpact(Vector3 direction, float force, float upwardForce = 0.5f)
    {
        // Yatay yönü normalleştir
        direction.y = 0f;
        direction.Normalize();

        // Yön + hafif yukarı doğru
        Vector3 knockDir = direction + Vector3.up * upwardForce;

        // Etkiyi ekle
        impact += knockDir * force;
    }
}
