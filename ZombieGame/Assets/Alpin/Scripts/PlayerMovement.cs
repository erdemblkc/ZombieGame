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

    // Animator'daki state isimleri (Animator penceresinde yazan isimlerle birebir aynı olmalı)
    private const string IdleStateName = "Idle";
    private const string WalkStateName = "walk";
    private const string WalkArmedStateName = "walk_gun"; // Silahlı yürüyüş state'i

    [Header("Model Root (Transform Sabitleme)")]
    public Transform modelRoot;                // Model Mesh objesi
    private Vector3 modelRootInitialScale;  // Başlangıç ölçeği (ör: 500,500,500)
    private Vector3 modelRootInitialPos;    // Başlangıç localPosition
    private Quaternion modelRootInitialRot;    // Başlangıç localRotation

    [Header("Knockback (Hasar Tepkisi)")]
    public float knockbackDamping = 5f; // Geri itilmeyi ne kadar hızlı sönümlensin

    [Header("Silah / Durum")]
    public bool isArmed = false;   // Silah alındı mı?

    private CharacterController controller;
    private Vector3 velocity;            // Zıplama + yerçekimi
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

        // Model transform'unu kaydet (animasyon ne yaparsa yapsın biz bunu geri yazacağız)
        if (modelRoot != null)
        {
            modelRootInitialScale = modelRoot.localScale;
            modelRootInitialPos = modelRoot.localPosition;
            modelRootInitialRot = modelRoot.localRotation;
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
        float vertical = Input.GetAxis("Vertical");    // W / S

        // Player'ın baktığı yöne göre hareket yönü
        Vector3 move = transform.right * horizontal + transform.forward * vertical;

        // Koşma: Left Shift basılıysa runSpeed, değilse walkSpeed
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float currSpeed = isRunning ? runSpeed : walkSpeed;

        // Yatay hareket (knockback hariç)
        controller.Move(move * currSpeed * Time.deltaTime);

        // === ANİMASYON KISMI ===
        if (animator != null)
        {
            // Hareket ediyor mu? (input var mı)
            bool isMoving = new Vector2(horizontal, vertical).sqrMagnitude > 0.01f;

            // Sadece "harekete başladı / durdu" anlarında state değiştir
            if (isMoving != lastIsMoving)
            {
                string targetState;

                if (isMoving)
                {
                    // Silahlı mı, silahsız mı?
                    targetState = isArmed ? WalkArmedStateName : WalkStateName;
                }
                else
                {
                    targetState = IdleStateName;
                }

                animator.CrossFade(targetState, 0.1f);
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
            knockbackVelocity = Vector3.Lerp(
                knockbackVelocity,
                Vector3.zero,
                knockbackDamping * Time.deltaTime
            );
        }
    }

    void LateUpdate()
    {
        // Animasyon Model Mesh'in Transform'u ile oynasa bile, her frame sonunda
        // başlangıç değerlerine geri çekiyoruz.
        if (modelRoot != null)
        {
            modelRoot.localScale = modelRootInitialScale;
            modelRoot.localPosition = modelRootInitialPos;
            modelRoot.localRotation = modelRootInitialRot;
        }
    }

    /// <summary>
    /// Zombie saldırdığında geri itme için PlayerHealth'ten çağrılıyor.
    /// </summary>
    public void AddImpact(Vector3 direction, float force)
    {
        direction.y = Mathf.Abs(direction.y);   // aşağı doğru vurulmasın
        direction.Normalize();

        knockbackVelocity = direction * force;
    }

    // Eski 3 parametreli çağrılar için overload
    public void AddImpact(Vector3 direction, float force, float extraUpForce)
    {
        direction.y += extraUpForce;
        AddImpact(direction, force);
    }

    /// <summary>
    /// Silah alındığında pickup script'i burayı çağıracak.
    /// </summary>
    public void SetArmed(bool armed)
    {
        isArmed = armed;
    }
}
