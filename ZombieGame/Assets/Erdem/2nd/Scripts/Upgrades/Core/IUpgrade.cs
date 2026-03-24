using UnityEngine;

/// <summary>
/// Contract for all upgrade behaviours. Each upgrade is a MonoBehaviour
/// dynamically added to the player by UpgradeSlotManager.
/// </summary>
public interface IUpgrade
{
    /// <summary>The ScriptableObject descriptor for this upgrade.</summary>
    UpgradeData Data { get; }

    /// <summary>True while this upgrade occupies an active slot and is not suppressed by an Evolution.</summary>
    bool IsActive { get; }

    /// <summary>Called by UpgradeSlotManager when the upgrade enters a slot.</summary>
    void OnUpgradeEnabled(GameObject player);

    /// <summary>Called by UpgradeSlotManager when the upgrade is removed from its slot.</summary>
    void OnUpgradeDisabled();

    /// <summary>
    /// Called by EvolutionRegistry to pause/resume this upgrade's behaviour
    /// while an Evolution that uses it is active.
    /// </summary>
    void SetEvolutionSuppressed(bool suppressed);
}
