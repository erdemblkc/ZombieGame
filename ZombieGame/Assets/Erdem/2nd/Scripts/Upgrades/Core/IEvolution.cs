using UnityEngine;

/// <summary>
/// Contract for Evolution behaviours. An Evolution activates when two specific
/// upgrades are simultaneously present in the player's upgrade slots.
/// </summary>
public interface IEvolution
{
    /// <summary>Called when both required upgrades are detected in the slot manager.</summary>
    void OnEvolutionEnabled(GameObject player);

    /// <summary>Called when either required upgrade is removed. Constituent upgrades resume normally.</summary>
    void OnEvolutionDisabled();
}
