using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController2 : MonoBehaviour
{
    [Header("Refs")]
    public Transform cameraPivot; // Main Camera (head altında)

    [Header("Movement")]
    public float moveSpeed = 4.5f;
    public float gravity = -18f;

    [Header("Mouse Look")]
    public float mouseSensitivityX = 1.0f;
    public float mouseSensitivityY = 0.6f;
    public float aimSensitivityY = 0.25f; // SADECE aim'de Y daha yavaş
    public float pitchMin = -85f;
    public float pitchMax = 85f;

    [Header("Animator Params")]
    public string speedParam = "Speed";
    public string aimingParam = "IsAiming";
    public string shootParam = "Shoot";

    [Header("Aim Pitch Bone")]
    public Transform upperBodyBone;                 // Chest / UpperChest / Spine
    public float upperBodyPitchMultiplier = 1.0f;   // sen 5 kullanıyorsan Inspector'dan 5 yap
    public float upperBodyPitchSmoothing = 30f;     // sen 50 kullanıyorsan Inspector'dan 50 yap
    public float upperBodyPitchClamp = 80f;         // aşırı kırılmayı önler

    private CharacterController cc;
    private Animator anim;

    private float pitch;
    private Vector3 verticalVel;
    private Quaternion upperBodyDefaultLocalRot;

    // Aim "snap" fix için
    private bool wasAiming;
    private float aimPitchStart;
    private Quaternion aimBoneStartRot;

    void Awake()
    {
        cc = GetComponent<CharacterController>();

        anim = GetComponentInChildren<Animator>();
        if (anim == null) anim = GetComponent<Animator>();
        if (anim != null) anim.applyRootMotion = false;

        if (cameraPivot == null && Camera.main != null)
            cameraPivot = Camera.main.transform;

        if (upperBodyBone != null)
            upperBodyDefaultLocalRot = upperBodyBone.localRotation;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // ✅ Sıra önemli: önce aim bool güncellensin
        HandleAimAndShoot();
        HandleMouseLook();
        HandleMovement();
        ApplyAimPitchToUpperBody();
    }

    void HandleMouseLook()
    {
        if (cameraPivot == null) return;

        bool isAiming = (anim != null) && anim.GetBool(aimingParam);

        float mx = Input.GetAxis("Mouse X") * mouseSensitivityX;
        float my = Input.GetAxis("Mouse Y") * (isAiming ? aimSensitivityY : mouseSensitivityY);

        // Yaw: karakteri döndür
        transform.Rotate(0f, mx, 0f);

        // Pitch: kamerayı döndür
        pitch -= my;
        pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);
        cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    void HandleAimAndShoot()
    {
        bool isAiming = Input.GetMouseButton(1);
        if (anim) anim.SetBool(aimingParam, isAiming);

        if (Input.GetMouseButtonDown(0))
        {
            if (anim) anim.SetTrigger(shootParam);
        }
    }

    void HandleMovement()
    {
        float x = Input.GetAxisRaw("Horizontal"); // A/D
        float z = Input.GetAxisRaw("Vertical");   // W/S

        Vector3 input = new Vector3(x, 0f, z);
        input = Vector3.ClampMagnitude(input, 1f);

        Vector3 moveDir = (transform.right * input.x + transform.forward * input.z);
        Vector3 move = moveDir * moveSpeed;

        // Gravity
        if (cc.isGrounded) verticalVel.y = -1f;
        else verticalVel.y += gravity * Time.deltaTime;

        cc.Move((move + verticalVel) * Time.deltaTime);

        // Animator Speed
        if (anim) anim.SetFloat(speedParam, input.magnitude);
    }

    void ApplyAimPitchToUpperBody()
    {
        if (upperBodyBone == null || anim == null) return;

        bool isAiming = anim.GetBool(aimingParam);

        // ✅ Aim'e bu frame girdiysek: referansı kilitle (snap olmasın)
        if (isAiming && !wasAiming)
        {
            aimPitchStart = pitch;                      // o anki kamera pitch
            aimBoneStartRot = upperBodyBone.localRotation; // o anki kemik rotu
        }

        // Aim yokken: kemiği eski haline döndür
        if (!isAiming)
        {
            upperBodyBone.localRotation = Quaternion.Slerp(
                upperBodyBone.localRotation,
                upperBodyDefaultLocalRot,
                upperBodyPitchSmoothing * Time.deltaTime
            );

            wasAiming = false;
            return;
        }

        // Aim varken: başlangıca göre delta uygula (baktığın yer sabit kalır)
        float deltaPitch = pitch - aimPitchStart;

        float applied = deltaPitch * upperBodyPitchMultiplier;
        applied = Mathf.Clamp(applied, -upperBodyPitchClamp, upperBodyPitchClamp);

        // Yön ters gelirse:
        // applied = -applied;

        Quaternion target = aimBoneStartRot * Quaternion.Euler(applied, 0f, 0f);

        upperBodyBone.localRotation = Quaternion.Slerp(
            upperBodyBone.localRotation,
            target,
            upperBodyPitchSmoothing * Time.deltaTime
        );

        wasAiming = true;
    }
}
