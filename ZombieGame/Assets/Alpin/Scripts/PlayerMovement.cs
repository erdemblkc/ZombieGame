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
    public Animator animator;        // Model Mesh üzerindeki Animator

    // Animator'daki state isimleri
    private const string IdleStateName = "Idle";
    private const string WalkStateName = "walk";

    [Header("Knockback (Hasar Tepkisi)")]
    public float knockbackDamping = 5f; // Geri itilmeyi ne kadar hızlı sönümlensin (5 iyi bir değer)

    private CharacterController controller;
    private Vector3 velocity;            // Sadece dikey hareket (zıplama + yerçekimi) için
    private Vector3 knockbackVelocity;   // Hasar alınca geri itme vektörü

    private bool lastIsMoving = false;   // Bir önceki frame'de hareket ediyor muydu?

    void Start()
    {
        controller = GetComponent<CharacterController>();

        // Eğer inspector'dan atamadıysan child'lardan Animator bul
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (animator != null)
        {
            animator.Play(IdleStateName, 0, 0f);
        }
        else
        {
            Debug.LogError("Player: Animator bulunamadı!");
        }
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

        // Yatay hareket (knockback hariç)
        controller.Move(move * currentSpeed * Time.deltaTime);

        // === ANİMASYON ===
        if (animator != null)
        {
            // Hareket ediyor mu? (input var mı)
            bool isMoving = new Vector2(horizontal, vertical).sqrMagnitude > 0.01f;

            // Sadece durum değiştiğinde state değiştir
            if (isMoving != lastIsMoving)
            {
                string targetState = isMoving ? WalkStateName : IdleStateName;
                animator.CrossFade(targetState, 0.1f);  // Idle <-> walk

                lastIsMoving = isMoving;
            }
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

        // === KNOCKBACK HAREKETİ ===
        if (knockbackVelocity.sqrMagnitude > 0.001f)
        {
            controller.Move(knockbackVelocity * Time.deltaTime);
            // Yavaş yavaş sıfıra çek
            knockbackVelocity = Vector3.Lerp(knockbackVelocity, Vector3.zero, knockbackDamping * Time.deltaTime);
        }
    }

    /// <summary>
    /// PlayerHealth tarafından çağrılan geri itme fonksiyonu.
    /// </summary>
    public void AddImpact(Vector3 direction, float force)
    {
        direction.y = Mathf.Abs(direction.y);
        direction.Normalize();
        knockbackVelocity = direction * force;
    }

    // 3 parametreli eski çağrı için
    public void AddImpact(Vector3 direction, float force, float extraUpForce)
    {
        direction.y += extraUpForce;
        AddImpact(direction, force);
    }
}

