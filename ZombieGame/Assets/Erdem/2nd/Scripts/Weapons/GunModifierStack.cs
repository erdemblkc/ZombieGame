using UnityEngine;

/// <summary>
/// Accumulates multiplicative modifiers from Gun upgrade behaviours.
/// GunShooter reads this every frame to apply effective damage, fire-rate, and special effects.
/// Attach to the Player GameObject alongside GunShooter.
/// </summary>
public class GunModifierStack : MonoBehaviour
{
    // ── Damage ────────────────────────────────────────────────────────────
    /// <summary>Final damage multiplier (product of all registered modifiers). Default = 1.</summary>
    public float DamageMultiplier { get; private set; } = 1f;

    // ── Fire Rate ─────────────────────────────────────────────────────────
    /// <summary>
    /// Multiplier applied to WeaponStats.fireCooldown.
    /// Values below 1 → faster fire rate; above 1 → slower.
    /// </summary>
    public float FireCooldownMultiplier { get; private set; } = 1f;

    // ── Special flags ─────────────────────────────────────────────────────
    /// <summary>When true GunShooter fires 2 extra angled bullets on each shot.</summary>
    public bool HasSpreadShot { get; private set; } = false;

    /// <summary>Bullets pierce this many additional targets after the first. 0 = normal.</summary>
    public int PiercingCount { get; private set; } = 0;

    // ── Registration API ──────────────────────────────────────────────────

    /// <summary>Called by a Gun upgrade on enable to register its multipliers.</summary>
    public void RegisterMultipliers(float damageMul, float fireCooldownMul)
    {
        DamageMultiplier      *= damageMul;
        FireCooldownMultiplier *= fireCooldownMul;
    }

    /// <summary>Called by a Gun upgrade on disable to remove its multipliers.</summary>
    public void UnregisterMultipliers(float damageMul, float fireCooldownMul)
    {
        if (damageMul       != 0f) DamageMultiplier      /= damageMul;
        if (fireCooldownMul != 0f) FireCooldownMultiplier /= fireCooldownMul;

        // Clamp to sane values to prevent floating-point drift
        DamageMultiplier      = Mathf.Max(0.01f, DamageMultiplier);
        FireCooldownMultiplier = Mathf.Max(0.01f, FireCooldownMultiplier);
    }

    /// <summary>Adds or removes the SpreadShot flag.</summary>
    public void SetSpreadShot(bool enabled) => HasSpreadShot = enabled;

    /// <summary>Increases or decreases the number of pierce targets.</summary>
    public void AddPiercing(int delta) => PiercingCount = Mathf.Max(0, PiercingCount + delta);

    // ── Ricochet ──────────────────────────────────────────────────────────
    /// <summary>Bullets bounce off walls this many times. 0 = no ricochet.</summary>
    public int RicochetCount { get; private set; } = 0;

    /// <summary>Increases or decreases the number of wall bounces.</summary>
    public void AddRicochet(int delta) => RicochetCount = Mathf.Max(0, RicochetCount + delta);
}
