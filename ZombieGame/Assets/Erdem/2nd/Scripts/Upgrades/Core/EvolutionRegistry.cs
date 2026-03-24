using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines which upgrade combinations trigger which Evolution behaviours.
/// Attach to the Player alongside UpgradeSlotManager.
/// </summary>
public class EvolutionRegistry : MonoBehaviour
{
    [Tooltip("Each entry defines an UpgradeA + UpgradeB pair and the Evolution type name to spawn.")]
    [SerializeField] private EvolutionEntry[] _entries = Array.Empty<EvolutionEntry>();

    private MonoBehaviour _activeEvolutionComp;
    private EvolutionEntry _activeEntry;
    private IUpgrade _activeA;
    private IUpgrade _activeB;

    // ── Public API called by UpgradeSlotManager ───────────────────────────

    /// <summary>Re-evaluate all entries against the current slot list. Call after any slot change.</summary>
    public void Evaluate(IUpgrade[] slots, GameObject player)
    {
        // 1. Check if active evolution is still valid
        if (_activeEvolutionComp != null)
        {
            bool stillValid = FindBoth(_activeEntry, slots, out _, out _);
            if (!stillValid)
                DeactivateCurrent();
        }

        // 2. Check for new evolution (only one at a time)
        if (_activeEvolutionComp == null)
        {
            foreach (var entry in _entries)
            {
                if (!FindBoth(entry, slots, out var a, out var b))
                    continue;

                var type = Type.GetType(entry.EvolutionTypeName);
                if (type == null)
                {
                    Debug.LogWarning($"[EvolutionRegistry] Could not find evolution type '{entry.EvolutionTypeName}'.");
                    continue;
                }

                _activeEvolutionComp = player.AddComponent(type) as MonoBehaviour;
                _activeEntry = entry;
                _activeA = a;
                _activeB = b;

                a.SetEvolutionSuppressed(true);
                b.SetEvolutionSuppressed(true);

                (_activeEvolutionComp as IEvolution)?.OnEvolutionEnabled(player);
                Debug.Log($"[EvolutionRegistry] Evolution '{entry.EvolutionTypeName}' activated.");
                break;
            }
        }
    }

    /// <summary>Called by UpgradeSlotManager before a specific upgrade is removed.</summary>
    public void NotifyUpgradeRemoving(IUpgrade removing)
    {
        if (_activeEvolutionComp == null) return;
        if (removing != _activeA && removing != _activeB) return;
        DeactivateCurrent();
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private void DeactivateCurrent()
    {
        if (_activeA != null) _activeA.SetEvolutionSuppressed(false);
        if (_activeB != null) _activeB.SetEvolutionSuppressed(false);

        (_activeEvolutionComp as IEvolution)?.OnEvolutionDisabled();
        Destroy(_activeEvolutionComp);

        _activeEvolutionComp = null;
        _activeEntry = null;
        _activeA = null;
        _activeB = null;
    }

    private bool FindBoth(EvolutionEntry entry, IUpgrade[] slots,
                          out IUpgrade foundA, out IUpgrade foundB)
    {
        foundA = null;
        foundB = null;
        foreach (var slot in slots)
        {
            if (slot == null) continue;
            if (slot.Data == entry.UpgradeA) foundA = slot;
            if (slot.Data == entry.UpgradeB) foundB = slot;
        }
        return foundA != null && foundB != null;
    }
}

// ── Data struct for Inspector configuration ───────────────────────────────────

[Serializable]
public class EvolutionEntry
{
    [Tooltip("First required upgrade.")]
    public UpgradeData UpgradeA;

    [Tooltip("Second required upgrade.")]
    public UpgradeData UpgradeB;

    [Tooltip("Full C# class name of the Evolution MonoBehaviour, e.g. 'ShoulderBashEvolution'.")]
    public string EvolutionTypeName;
}
