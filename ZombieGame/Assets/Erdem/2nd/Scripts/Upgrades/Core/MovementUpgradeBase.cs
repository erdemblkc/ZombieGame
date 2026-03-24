using UnityEngine;

/// <summary>
/// Abstract base class for all Movement upgrade behaviours.
/// Handles common setup (component caching), lifecycle (IsActive, evolution suppression)
/// and exposes IsRunnable so subclasses skip their Update logic cleanly.
/// </summary>
public abstract class MovementUpgradeBase : MonoBehaviour, IUpgrade
{
    // ── IUpgrade ──────────────────────────────────────────────────────────
    private UpgradeData _data;
    private bool _isActive;
    private bool _evolutionSuppressed;

    public UpgradeData Data    => _data;
    public bool        IsActive => _isActive;

    /// <summary>True only when the upgrade is active AND not paused by an Evolution.</summary>
    protected bool IsRunnable => _isActive && !_evolutionSuppressed;

    // ── Cached player components ──────────────────────────────────────────
    protected PlayerController2      _player;
    protected CharacterController    _cc;
    protected PlayerMovementModifiers _mods;
    protected Camera                 _cam;

    // ── Lifecycle ─────────────────────────────────────────────────────────

    /// <summary>Called by UpgradeSlotManager after AddComponent. Caches references and activates.</summary>
    public virtual void OnUpgradeEnabled(GameObject player)
    {
        _player = player.GetComponent<PlayerController2>();
        _cc     = player.GetComponent<CharacterController>();
        _mods   = player.GetComponent<PlayerMovementModifiers>();
        _cam    = Camera.main;
        _isActive = true;
        enabled   = true;
    }

    /// <summary>Called by UpgradeSlotManager before Destroy. Must clean up all modifier flags.</summary>
    public virtual void OnUpgradeDisabled()
    {
        _isActive = false;
        enabled   = false;
    }

    /// <summary>Pauses/resumes this upgrade's Update behaviour when an Evolution is active.</summary>
    public void SetEvolutionSuppressed(bool suppressed) => _evolutionSuppressed = suppressed;

    // ── Internal helpers ──────────────────────────────────────────────────

    /// <summary>Called by UpgradeSlotManager to bind the UpgradeData descriptor.</summary>
    internal void SetData(UpgradeData data) => _data = data;

    // Start disabled — OnUpgradeEnabled sets enabled = true.
    void Awake() => enabled = false;
}
