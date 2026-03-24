using UnityEngine;
using System.Collections;

/// <summary>
/// Wall Run — when airborne and pressing forward near a wall,
/// the player runs along the wall surface for up to wallRunDuration seconds.
/// A wall kick jump is available while running.
/// </summary>
public class WallRunUpgrade : MovementUpgradeBase
{
    [Header("Wall Run Parameters")]
    [SerializeField] private float   _wallRunDuration  = 2f;
    [SerializeField] private float   _wallRunSpeed     = 10f;
    [SerializeField] private float   _wallDetectDist   = 0.8f;
    [SerializeField] private float   _wallKickUpForce  = 8f;
    [SerializeField] private float   _wallKickSideDur  = 0.18f;
    [SerializeField] private LayerMask _wallLayer      = ~0;   // all layers by default

    private bool    _isWallRunning;
    private float   _wallRunTimer;
    private float   _wallExitCooldown;  // brief cooldown to prevent instant re-attach
    private Vector3 _wallNormal;
    private Vector3 _wallForward;

    public bool IsWallRunning => _isWallRunning;

    public override void OnUpgradeDisabled()
    {
        if (_isWallRunning) EndWallRun(false);
        if (_mods != null)
        {
            _mods.SuppressHorizontalMove = false;
            _mods.SuppressGravity        = false;
        }
        base.OnUpgradeDisabled();
    }

    void Update()
    {
        if (!IsRunnable) return;

        if (_wallExitCooldown > 0f)
            _wallExitCooldown -= Time.deltaTime;

        if (_isWallRunning)
        {
            WallRunTick();
        }
        else if (!_cc.isGrounded && _wallExitCooldown <= 0f)
        {
            TryStartWallRun();
        }
    }

    void TryStartWallRun()
    {
        // Must be moving forward
        if (Input.GetAxisRaw("Vertical") < 0.5f) return;

        RaycastHit hit;
        bool right = Physics.Raycast(_cc.transform.position, _cc.transform.right,  out hit, _wallDetectDist, _wallLayer);
        bool left  = !right && Physics.Raycast(_cc.transform.position, -_cc.transform.right, out hit, _wallDetectDist, _wallLayer);

        if (!right && !left) return;

        // Don't wall-run on floors/ceilings
        if (Mathf.Abs(hit.normal.y) > 0.4f) return;

        _wallNormal   = hit.normal;
        _wallForward  = Vector3.Cross(_wallNormal, Vector3.up).normalized;

        // Align wall forward with player facing
        if (Vector3.Dot(_wallForward, _cc.transform.forward) < 0f)
            _wallForward = -_wallForward;

        _isWallRunning = true;
        _wallRunTimer  = _wallRunDuration;

        _mods.SuppressHorizontalMove = true;
        _mods.SuppressGravity        = true;
        _player.ResetVerticalVelocity();
    }

    void WallRunTick()
    {
        // Confirm wall still exists
        RaycastHit hit;
        bool wallRight = Physics.Raycast(_cc.transform.position, _cc.transform.right,  out hit, _wallDetectDist + 0.2f, _wallLayer);
        bool wallLeft  = !wallRight && Physics.Raycast(_cc.transform.position, -_cc.transform.right, out hit, _wallDetectDist + 0.2f, _wallLayer);

        if (!wallRight && !wallLeft)
        {
            EndWallRun(false);
            return;
        }

        _wallRunTimer -= Time.deltaTime;
        if (_wallRunTimer <= 0f || _cc.isGrounded)
        {
            EndWallRun(_cc.isGrounded);
            return;
        }

        // Move along wall
        _cc.Move(_wallForward * _wallRunSpeed * Time.deltaTime);

        // Wall kick
        if (Input.GetKeyDown(KeyCode.Space))
        {
            EndWallRun(false);
            StartCoroutine(WallKickCoroutine());
        }
    }

    void EndWallRun(bool landedOnGround)
    {
        _isWallRunning               = false;
        _wallExitCooldown            = 0.3f;
        _mods.SuppressHorizontalMove = false;
        _mods.SuppressGravity        = false;
    }

    IEnumerator WallKickCoroutine()
    {
        // Brief jump away from wall: suppress horizontal briefly and push
        _mods.SuppressHorizontalMove = true;
        _mods.SuppressGravity        = true;
        _player.ResetVerticalVelocity();

        Vector3 kickDir = (_wallNormal + Vector3.up * 2f).normalized;
        float elapsed = 0f;

        while (elapsed < _wallKickSideDur)
        {
            elapsed += Time.deltaTime;
            _cc.Move(kickDir * _wallKickUpForce * Time.deltaTime);
            yield return null;
        }

        _mods.SuppressHorizontalMove = false;
        _mods.SuppressGravity        = false;
    }
}
