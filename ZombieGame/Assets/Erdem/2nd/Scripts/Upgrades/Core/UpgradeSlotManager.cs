using UnityEngine;

/// <summary>
/// Manages the player's 4 upgrade slots. Handles dynamic addition/removal of
/// upgrade MonoBehaviours and delegates to EvolutionRegistry after each change.
/// Attach to the Player GameObject.
/// </summary>
public class UpgradeSlotManager : MonoBehaviour
{
    public const int SlotCount = 4;

    private readonly UpgradeData[] _slots          = new UpgradeData[SlotCount];
    private readonly IUpgrade[]    _behaviours      = new IUpgrade[SlotCount];

    private EvolutionRegistry _evolutionRegistry;

    void Awake()
    {
        _evolutionRegistry = GetComponent<EvolutionRegistry>();
    }

    // ── Public API ────────────────────────────────────────────────────────

    /// <summary>
    /// Adds an upgrade to the given slot index (0–3).
    /// Returns false if slot is occupied, upgrade already installed, or type lookup fails.
    /// </summary>
    public bool AddUpgrade(int slotIndex, UpgradeData data)
    {
        if (slotIndex < 0 || slotIndex >= SlotCount)
        {
            Debug.LogWarning($"[UpgradeSlotManager] Slot index {slotIndex} out of range.");
            return false;
        }
        if (_slots[slotIndex] != null)
        {
            Debug.LogWarning($"[UpgradeSlotManager] Slot {slotIndex} is already occupied.");
            return false;
        }
        if (HasUpgrade(data))
        {
            Debug.LogWarning($"[UpgradeSlotManager] Upgrade '{data.UpgradeName}' is already installed.");
            return false;
        }

        var type = data.GetBehaviourType();
        if (type == null)
        {
            Debug.LogError($"[UpgradeSlotManager] Could not find type '{data.UpgradeName}'. Check BehaviourTypeName in UpgradeData.");
            return false;
        }

        var comp    = gameObject.AddComponent(type);
        var upgrade = comp as IUpgrade;
        if (upgrade == null)
        {
            Destroy(comp);
            Debug.LogError($"[UpgradeSlotManager] Type '{type.Name}' does not implement IUpgrade.");
            return false;
        }

        if (comp is MovementUpgradeBase mub)
            mub.SetData(data);

        _slots[slotIndex]      = data;
        _behaviours[slotIndex] = upgrade;

        upgrade.OnUpgradeEnabled(gameObject);

        _evolutionRegistry?.Evaluate(_behaviours, gameObject);
        return true;
    }

    /// <summary>Removes the upgrade from the given slot. Returns false if slot is empty.</summary>
    public bool RemoveUpgrade(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= SlotCount || _slots[slotIndex] == null)
            return false;

        // Disable any active evolution first (so it can clean up)
        _evolutionRegistry?.NotifyUpgradeRemoving(_behaviours[slotIndex]);

        var upgrade = _behaviours[slotIndex];
        upgrade.OnUpgradeDisabled();

        if (upgrade is Component comp)
            Destroy(comp);

        _slots[slotIndex]      = null;
        _behaviours[slotIndex] = null;

        _evolutionRegistry?.Evaluate(_behaviours, gameObject);
        return true;
    }

    /// <summary>Returns true if an upgrade matching the given data is installed in any slot.</summary>
    public bool HasUpgrade(UpgradeData data)
    {
        for (int i = 0; i < SlotCount; i++)
            if (_slots[i] == data) return true;
        return false;
    }

    /// <summary>Returns the UpgradeData in the given slot, or null if empty.</summary>
    public UpgradeData GetSlot(int slotIndex) =>
        (slotIndex >= 0 && slotIndex < SlotCount) ? _slots[slotIndex] : null;

    /// <summary>Returns the IUpgrade behaviour in the given slot, or null.</summary>
    public IUpgrade GetBehaviour(int slotIndex) =>
        (slotIndex >= 0 && slotIndex < SlotCount) ? _behaviours[slotIndex] : null;

    /// <summary>Removes all upgrades from all slots.</summary>
    public void ClearAll()
    {
        for (int i = SlotCount - 1; i >= 0; i--)
            if (_slots[i] != null)
                RemoveUpgrade(i);
    }
}
