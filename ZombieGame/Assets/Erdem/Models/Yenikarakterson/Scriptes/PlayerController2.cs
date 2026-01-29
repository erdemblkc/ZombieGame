using UnityEngine;
using System.Collections.Generic; // List kullanımı için

[RequireComponent(typeof(CharacterController))]
public class PlayerController2 : MonoBehaviour
{
    [Header("Refs")]
    public Transform cameraPivot;

    [Header("Audio - Footsteps (YENİ)")]
    public AudioSource footstepSource; // Inspector'dan ata
    public List<AudioClip> footstepSounds; // Yürüme seslerini buraya ekle (birden fazla olabilir)
    public float stepInterval = 0.5f; // Ne sıklıkla ses çıksın?
    public float runStepMultiplier = 0.6f; // Koşarken süre ne kadar kısalsın? (0.6 katı)
    [Range(0f, 1f)] public float footstepVolume = 0.6f;

    [Header("Movement")]
    public float moveSpeed = 6.5f;
    public float gravity = -18f;

    [Header("Sprint (Shift)")]
    public KeyCode sprintKey = KeyCode.LeftShift;
    public float sprintSpeed = 9.5f;
    public float maxEnergy = 10f;
    public float energyDrainPerSec = 2.5f;
    public float energyRefillTime = 5f;
    public float sprintExhaustLockTime = 3f;

    [Header("Jump")]
    public float jumpHeight = 1.3f;
    public float groundedStickForce = -2f;

    [Header("Dash (E)")]
    public KeyCode dashKey = KeyCode.E;
    public float dashSpeed = 14f;
    public float dashDuration = 0.18f;
    public float dashCooldown = 0.9f;
    public bool dashOnlyForward = false;

    [Header("Mouse Look")]
    public float mouseSensitivityX = 1.0f;
    public float mouseSensitivityY = 0.6f;
    public float aimSensitivityY = 0.25f;
    public float pitchMin = -85f;
    public float pitchMax = 85f;

    [Header("Animator Params")]
    public string speedParam = "Speed";
    public string aimingParam = "IsAiming";
    public string shootParam = "Shoot";

    [Header("Aim Pitch Bone")]
    public Transform upperBodyBone;
    public float upperBodyPitchMultiplier = 1.0f;
    public float upperBodyPitchSmoothing = 30f;
    public float upperBodyPitchClamp = 80f;

    // ---------------- KNOCKBACK / IMPULSE ----------------
    [Header("Knockback (Impulse)")]
    public bool enableImpulse = true;
    public float impulseHorizontal = 7.5f;
    public float impulseUp = 2.5f;
    public float impulseDuration = 0.12f;
    public float impulseDamping = 18f;

    private Vector3 impulseVel;
    private float impulseTimer;

    public void AddKnockbackFrom(Vector3 attackerPosition)
    {
        if (!enableImpulse) return;
        Vector3 dir = (transform.position - attackerPosition);
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) dir = transform.forward;
        dir.Normalize();
        impulseVel = dir * impulseHorizontal + Vector3.up * impulseUp;
        impulseTimer = impulseDuration;
    }

    private CharacterController cc;
    private Animator anim;
    private float pitch;
    private Vector3 verticalVel;
    private Quaternion upperBodyDefaultLocalRot;

    // Aim "snap" fix
    private bool wasAiming;
    private float aimPitchStart;
    private Quaternion aimBoneStartRot;

    // Dash state
    private bool isDashing;
    private float dashTimer;
    private float nextDashTime;

    // Energy
    private float currentEnergy;
    public float CurrentEnergy => currentEnergy;
    public float MaxEnergy => maxEnergy;
    private float sprintLockTimer;

    // Audio Timer
    private float footstepTimer;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        anim = GetComponentInChildren<Animator>();
        if (anim == null) anim = GetComponent<Animator>();
        if (anim != null) anim.applyRootMotion = false;

        if (cameraPivot == null && Camera.main != null)
            cameraPivot = Camera.main.transform;

        // AudioSource otomatik bulma
        if (footstepSource == null) footstepSource = GetComponent<AudioSource>();

        if (upperBodyBone != null)
            upperBodyDefaultLocalRot = upperBodyBone.localRotation;

        currentEnergy = maxEnergy;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleAimAndShoot();
        HandleMouseLook();

        HandleDash();
        HandleMovement();
        ApplyAimPitchToUpperBody();

        if (sprintLockTimer > 0f)
            sprintLockTimer -= Time.deltaTime;
    }

    void HandleMouseLook()
    {
        if (cameraPivot == null) return;
        bool isAiming = (anim != null) && anim.GetBool(aimingParam);
        float mx = Input.GetAxis("Mouse X") * mouseSensitivityX;
        float my = Input.GetAxis("Mouse Y") * (isAiming ? aimSensitivityY : mouseSensitivityY);

        transform.Rotate(0f, mx, 0f);
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

    void HandleDash()
    {
        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f) isDashing = false;
            return;
        }

        if (Input.GetKeyDown(dashKey) && Time.time >= nextDashTime)
        {
            isDashing = true;
            dashTimer = dashDuration;
            nextDashTime = Time.time + dashCooldown;
        }
    }

    void HandleMovement()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 input = new Vector3(x, 0f, z);
        input = Vector3.ClampMagnitude(input, 1f);
        Vector3 moveDir = (transform.right * input.x + transform.forward * input.z);

        // Gravity + Jump
        if (cc.isGrounded)
        {
            if (verticalVel.y < 0f) verticalVel.y = groundedStickForce;
            if (Input.GetKeyDown(KeyCode.Space))
                verticalVel.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        else
        {
            verticalVel.y += gravity * Time.deltaTime;
        }

        // DASH
        if (isDashing)
        {
            Vector3 dashDir = dashOnlyForward ? transform.forward : (moveDir.sqrMagnitude > 0.001f ? moveDir : transform.forward);
            dashDir.Normalize();
            Vector3 dashMove = (dashDir * dashSpeed + verticalVel) + ConsumeImpulse();
            cc.Move(dashMove * Time.deltaTime);
            if (anim) anim.SetFloat(speedParam, 1f);
            return;
        }

        // SPRINT
        bool wantsSprint = Input.GetKey(sprintKey);
        bool isMoving = input.magnitude > 0.01f;
        bool isSprinting = wantsSprint && isMoving && sprintLockTimer <= 0f && currentEnergy > 0.01f;

        if (isSprinting)
        {
            currentEnergy -= energyDrainPerSec * Time.deltaTime;
            if (currentEnergy <= 0f)
            {
                currentEnergy = 0f;
                sprintLockTimer = sprintExhaustLockTime;
                isSprinting = false;
            }
        }
        else
        {
            float regenPerSec = (energyRefillTime <= 0.001f) ? 999f : (maxEnergy / energyRefillTime);
            currentEnergy += regenPerSec * Time.deltaTime;
            if (currentEnergy > maxEnergy) currentEnergy = maxEnergy;
        }

        float speed = isSprinting ? sprintSpeed : moveSpeed;
        Vector3 move = moveDir * speed;
        Vector3 finalMove = move + verticalVel + ConsumeImpulse();
        cc.Move(finalMove * Time.deltaTime);

        if (anim) anim.SetFloat(speedParam, input.magnitude);

        // --- FOOTSTEP AUDIO LOGIC (YENİ) ---
        HandleFootsteps(isMoving, isSprinting);
    }

    void HandleFootsteps(bool isMoving, bool isSprinting)
    {
        if (!cc.isGrounded) return; // Havadaysa ses yok
        if (!isMoving) return;      // Duruyorsa ses yok

        // Süreyi belirle (koşuyorsa daha sık)
        float currentInterval = isSprinting ? (stepInterval * runStepMultiplier) : stepInterval;

        footstepTimer -= Time.deltaTime;

        if (footstepTimer <= 0f)
        {
            PlayRandomFootstep();
            footstepTimer = currentInterval;
        }
    }

    void PlayRandomFootstep()
    {
        if (footstepSource == null || footstepSounds == null || footstepSounds.Count == 0) return;

        // Listeden rastgele bir ses seç
        int n = Random.Range(0, footstepSounds.Count);
        AudioClip clip = footstepSounds[n];

        // Pitch ve Volume ile hafif varyasyon yap (robotik duyulmasın)
        footstepSource.pitch = Random.Range(0.9f, 1.1f);
        footstepSource.PlayOneShot(clip, footstepVolume);
    }

    Vector3 ConsumeImpulse()
    {
        if (!enableImpulse) return Vector3.zero;
        if (impulseTimer <= 0f)
        {
            impulseVel = Vector3.Lerp(impulseVel, Vector3.zero, impulseDamping * Time.deltaTime);
            if (impulseVel.sqrMagnitude < 0.0001f) impulseVel = Vector3.zero;
            return Vector3.zero;
        }
        impulseTimer -= Time.deltaTime;
        Vector3 v = impulseVel;
        impulseVel = Vector3.Lerp(impulseVel, Vector3.zero, impulseDamping * Time.deltaTime);
        return v;
    }

    void ApplyAimPitchToUpperBody()
    {
        if (upperBodyBone == null || anim == null) return;
        bool isAiming = anim.GetBool(aimingParam);
        if (isAiming && !wasAiming)
        {
            aimPitchStart = pitch;
            aimBoneStartRot = upperBodyBone.localRotation;
        }
        if (!isAiming)
        {
            upperBodyBone.localRotation = Quaternion.Slerp(upperBodyBone.localRotation, upperBodyDefaultLocalRot, upperBodyPitchSmoothing * Time.deltaTime);
            wasAiming = false;
            return;
        }
        float deltaPitch = pitch - aimPitchStart;
        float applied = deltaPitch * upperBodyPitchMultiplier;
        applied = Mathf.Clamp(applied, -upperBodyPitchClamp, upperBodyPitchClamp);
        Quaternion target = aimBoneStartRot * Quaternion.Euler(applied, 0f, 0f);
        upperBodyBone.localRotation = Quaternion.Slerp(upperBodyBone.localRotation, target, upperBodyPitchSmoothing * Time.deltaTime);
        wasAiming = true;
    }
}