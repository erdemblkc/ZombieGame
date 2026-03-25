using UnityEngine;

/// <summary>
/// Abstract base class for all Gun upgrade behaviours.
/// Caches GunShooter, GunModifierStack, and PlayerDamageReceiver on enable,
/// and handles the standard IUpgrade lifecycle.
/// </summary>
public abstract class GunUpgradeBase : MonoBehaviour, IUpgrade
{
    // ── IUpgrade ──────────────────────────────────────────────────────────
    private UpgradeData _data;
    private bool _isActive;
    private bool _evolutionSuppressed;

    public UpgradeData Data    => _data;
    public bool        IsActive => _isActive;

    /// <summary>True only when the upgrade is active AND not suppressed by an Evolution.</summary>
    protected bool IsRunnable => _isActive && !_evolutionSuppressed;

    // ── Cached player components ──────────────────────────────────────────
    protected GunShooter          _shooter;
    protected GunModifierStack    _modStack;
    protected PlayerDamageReceiver _player;

    // ── Lifecycle ─────────────────────────────────────────────────────────

    /// <summary>Override to apply stat changes when the upgrade activates.</summary>
    public virtual void OnUpgradeEnabled(GameObject player)
    {
        _shooter  = player.GetComponent<GunShooter>();
        _modStack = player.GetComponent<GunModifierStack>();
        _player   = player.GetComponent<PlayerDamageReceiver>();
        _isActive = true;
        enabled   = true;
    }

    /// <summary>Override to revert all changes when the upgrade is removed.</summary>
    public virtual void OnUpgradeDisabled()
    {
        _isActive = false;
        enabled   = false;
    }

    public void SetEvolutionSuppressed(bool suppressed) => _evolutionSuppressed = suppressed;

    internal void SetData(UpgradeData data) => _data = data;

    // Start disabled — OnUpgradeEnabled sets enabled = true.
    void Awake() => enabled = false;
}
