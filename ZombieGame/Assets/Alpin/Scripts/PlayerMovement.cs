using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Hız Ayarları")]
    public float walkSpeed = 5f;
    public float runSpeed = 9f; // artık kullanılmıyor

    [Header("Dash (Shift)")]
    public float dashSpeed = 14f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 0.8f;

    [Header("Aim (RMB)")]
    public KeyCode aimKey = KeyCode.Mouse1;

    [Header("Fizik")]
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;

    [Header("Animasyon")]
    public Animator animator;

    // SENİN Animator state isimlerin
    private const string IdleStateName = "Armature|Idle";
    private const string WalkStateName = "Armature|Walk";
    private const string AimIdleStateName = "Armature|AimIdle";
    private const string AimWalkStateName = "walk_aim";

    [Header("Model Root (Transform Sabitleme)")]
    public Transform modelRoot; // Inspector: Model Mesh (Transform)
    public bool lockModelRootTransform = true;

    [Tooltip("Animasyon scale'i bozuyorsa bunu aç ve 500,500,500 gir.")]
    public bool useManualModelScale = false;
    public Vector3 manualModelScale = new Vector3(500f, 500f, 500f);

    private Vector3 modelRootLockedScale;
    private Vector3 modelRootLockedPos;
    private Quaternion modelRootLockedRot;

    [Header("Knockback (Hasar Tepkisi)")]
    public float knockbackDamping = 5f;

    [Header("Silah / Durum")]
    public bool isArmed = true;

    private CharacterController controller;
    private Vector3 velocity;
    private Vector3 knockbackVelocity;

    // Aim + Dash
    private bool isAiming;
    private bool isDashing;
    private float dashEndTime;
    private float nextDashTime;
    private Vector3 dashDir;

    // Anim state kontrol
    private string lastState = "";

    void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (modelRoot == null && animator != null)
            modelRoot = animator.transform;

        if (modelRoot != null && lockModelRootTransform)
        {
            modelRootLockedScale = useManualModelScale ? manualModelScale : modelRoot.localScale;
            modelRootLockedPos = modelRoot.localPosition;
            modelRootLockedRot = modelRoot.localRotation;
        }
    }

    void Start()
    {
        if (animator == null)
            Debug.LogError("Player: Animator bulunamadı!");
        else
            animator.Play(IdleStateName, 0, 0f);
    }

    void Update()
    {
        bool isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0) velocity.y = -2f;

        // Aim
        isAiming = isArmed && Input.GetKey(aimKey);
        if (animator != null) animator.SetBool("IsAiming", isAiming);

        // Input (Move için smooth, isMoving için RAW)
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        float hRaw = Input.GetAxisRaw("Horizontal");
        float vRaw = Input.GetAxisRaw("Vertical");
        bool isMoving = (hRaw * hRaw + vRaw * vRaw) > 0.01f;

        Vector3 move = transform.right * horizontal + transform.forward * vertical;

        // tuş bırakınca kayma/yanlış moving algısı olmasın
        if (!isMoving)
            move = Vector3.zero;

        // Dash (Shift)
        if (!isDashing && Time.time >= nextDashTime && Input.GetKeyDown(KeyCode.LeftShift))
        {
            dashDir = move.sqrMagnitude > 0.001f ? move.normalized : transform.forward;
            isDashing = true;
            dashEndTime = Time.time + dashDuration;
            nextDashTime = Time.time + dashCooldown;
        }

        // Normal yürüyüş
        controller.Move(move * walkSpeed * Time.deltaTime);

        // Dash hareketi
        if (isDashing)
        {
            if (Time.time >= dashEndTime) isDashing = false;
            else controller.Move(dashDir * dashSpeed * Time.deltaTime);
        }

        // Jump
        if (isGrounded && Input.GetButtonDown("Jump"))
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        // Gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // Knockback
        if (knockbackVelocity.sqrMagnitude > 0.001f)
        {
            controller.Move(knockbackVelocity * Time.deltaTime);
            knockbackVelocity = Vector3.Lerp(knockbackVelocity, Vector3.zero, knockbackDamping * Time.deltaTime);
        }

        // === ANİMASYON STATE SEÇİMİ ===
        if (animator != null)
        {
            string targetState;
            if (!isMoving)
                targetState = isAiming ? AimIdleStateName : IdleStateName;
            else
                targetState = isAiming ? AimWalkStateName : WalkStateName;

            // AimWalk klibi loop değilse "ilk adım sonra donma"yı engelle (fallback)
            // (Yine de asıl çözüm: AimWalk clip -> Loop Time ✅)
            if (targetState == AimWalkStateName && lastState == AimWalkStateName && isMoving && isAiming)
            {
                var st = animator.GetCurrentAnimatorStateInfo(0);
                if (st.IsName(AimWalkStateName) && st.normalizedTime >= 0.98f)
                {
                    var clips = animator.GetCurrentAnimatorClipInfo(0);
                    if (clips.Length > 0 && clips[0].clip != null && !clips[0].clip.isLooping)
                    {
                        animator.CrossFade(AimWalkStateName, 0.05f, 0, 0f); // yeniden başlat
                    }
                }
            }

            if (targetState != lastState)
            {
                // Idle'lara girerken poz "walk'tan kalmasın" diye 0'dan başlat
                if (targetState == IdleStateName || targetState == AimIdleStateName)
                    animator.CrossFade(targetState, 0.1f, 0, 0f);
                else
                    animator.CrossFade(targetState, 0.1f);

                lastState = targetState;
            }
        }
    }

    void LateUpdate()
    {
        if (modelRoot != null && lockModelRootTransform)
        {
            modelRoot.localScale = useManualModelScale ? manualModelScale : modelRootLockedScale;
            modelRoot.localPosition = modelRootLockedPos;
            modelRoot.localRotation = modelRootLockedRot;
        }
    }

    public void AddImpact(Vector3 direction, float force)
    {
        direction.y = Mathf.Abs(direction.y);
        direction.Normalize();
        knockbackVelocity = direction * force;
    }

    public void AddImpact(Vector3 direction, float force, float extraUpForce)
    {
        direction.y += extraUpForce;
        AddImpact(direction, force);
    }

    public void SetArmed(bool armed)
    {
        isArmed = armed;
    }
}
