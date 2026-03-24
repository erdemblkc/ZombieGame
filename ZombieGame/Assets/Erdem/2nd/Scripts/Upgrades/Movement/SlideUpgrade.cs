using UnityEngine;
using System.Collections;

/// <summary>
/// Slide — while sprinting, press C to slide forward at high speed.
/// Shrinks the CharacterController height for low clearances.
/// Grants brief invincibility frames at slide start.
/// </summary>
public class SlideUpgrade : MovementUpgradeBase
{
    [Header("Slide Parameters")]
    [SerializeField] private float _slideSpeed     = 14f;
    [SerializeField] private float _slideDuration  = 0.6f;
    [SerializeField] private float _iFrameDuration = 0.2f;
    [SerializeField] private float _cooldown       = 4f;
    [SerializeField] private KeyCode _slideKey     = KeyCode.C;

    private bool    _isSliding;
    private float   _slideTimer;
    private float   _cooldownTimer;
    private Vector3 _slideDir;
    private float   _originalHeight;
    private Vector3 _originalCenter;

    // ── Invincibility hook (read by PlayerDamageReceiver if desired) ───────
    public bool IsInvincible { get; private set; }

    public bool IsSliding => _isSliding;

    public override void OnUpgradeEnabled(GameObject player)
    {
        base.OnUpgradeEnabled(player);
        _originalHeight = _cc.height;
        _originalCenter = _cc.center;
    }

    public override void OnUpgradeDisabled()
    {
        if (_isSliding) EndSlide();
        if (_mods != null) _mods.SuppressHorizontalMove = false;
        base.OnUpgradeDisabled();
    }

    void Update()
    {
        if (!IsRunnable) return;

        if (_cooldownTimer > 0f)
            _cooldownTimer -= Time.deltaTime;

        if (_isSliding)
        {
            _slideTimer -= Time.deltaTime;
            IsInvincible = _slideTimer > (_slideDuration - _iFrameDuration);

            _mods.SuppressHorizontalMove = true;
            _cc.Move(_slideDir * _slideSpeed * Time.deltaTime);

            if (_slideTimer <= 0f)
                EndSlide();

            return;
        }

        // Trigger: sprint + C + cooldown ready
        if (_cooldownTimer <= 0f
            && _player.IsSprinting
            && Input.GetKeyDown(_slideKey))
        {
            StartSlide();
        }
    }

    void StartSlide()
    {
        _isSliding  = true;
        _slideTimer = _slideDuration;
        _slideDir   = _cc.transform.forward;

        // Halve the collider height
        _cc.height = _originalHeight * 0.5f;
        _cc.center = new Vector3(_originalCenter.x, _originalHeight * 0.25f, _originalCenter.z);
    }

    void EndSlide()
    {
        _isSliding   = false;
        IsInvincible = false;
        _cooldownTimer = _cooldown;

        // Restore collider
        _cc.height = _originalHeight;
        _cc.center = _originalCenter;

        _mods.SuppressHorizontalMove = false;
    }
}
