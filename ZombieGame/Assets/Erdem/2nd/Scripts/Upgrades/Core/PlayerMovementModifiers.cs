using UnityEngine;

/// <summary>
/// Shared modifier flags written by upgrade behaviours and read by PlayerController2.
/// Attach this component to the Player alongside PlayerController2.
/// Upgrades must restore their flags in OnUpgradeDisabled() to leave no side-effects.
/// </summary>
public class PlayerMovementModifiers : MonoBehaviour
{
    // ── Speed ──────────────────────────────────────────────────────────────
    /// <summary>Multiplier applied to base move/sprint speed. 1 = no change.</summary>
    [HideInInspector] public float SpeedMultiplier = 1f;

    // ── Suppression flags ──────────────────────────────────────────────────
    /// <summary>PlayerController2 skips its own dash input — DoubleDash upgrade takes over.</summary>
    [HideInInspector] public bool SuppressDash = false;

    /// <summary>PlayerController2 skips horizontal movement — an upgrade drives lateral movement.</summary>
    [HideInInspector] public bool SuppressHorizontalMove = false;

    /// <summary>PlayerController2 does not apply gravity or update verticalVel — an upgrade owns vertical.</summary>
    [HideInInspector] public bool SuppressGravity = false;

    /// <summary>PlayerController2 ignores Space jump input — an upgrade handles jumping.</summary>
    [HideInInspector] public bool SuppressJump = false;

    void Awake() => ResetAll();

    /// <summary>Resets all modifiers to neutral defaults. Called on scene load.</summary>
    public void ResetAll()
    {
        SpeedMultiplier        = 1f;
        SuppressDash           = false;
        SuppressHorizontalMove = false;
        SuppressGravity        = false;
        SuppressJump           = false;
    }
}
