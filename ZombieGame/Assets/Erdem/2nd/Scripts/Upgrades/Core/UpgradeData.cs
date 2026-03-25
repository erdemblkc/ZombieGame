using UnityEngine;

/// <summary>Upgrade category used for filtering and display.</summary>
public enum UpgradeCategory { Movement, Gun, Utility, Evolution }

/// <summary>
/// ScriptableObject that describes an upgrade: display info + the MonoBehaviour
/// class name that implements the behaviour. Create one .asset per upgrade via
/// Assets → Create → Upgrades → UpgradeData.
/// </summary>
[CreateAssetMenu(fileName = "NewUpgrade", menuName = "Upgrades/UpgradeData")]
public class UpgradeData : ScriptableObject
{
    [SerializeField] private string _upgradeName = "Unnamed Upgrade";

    [SerializeField] private Sprite _icon;

    [TextArea(2, 4)]
    [SerializeField] private string _description = "";

    [SerializeField] private UpgradeCategory _category = UpgradeCategory.Movement;

    [Tooltip("Full C# class name of the MonoBehaviour that implements this upgrade.\n" +
             "Example: JetpackUpgrade")]
    [SerializeField] private string _behaviourTypeName = "";

    [Tooltip("For EVOLUTION upgrades: the player must already own all upgrades in this list.\n" +
             "Leave empty for non-evolution upgrades.")]
    [SerializeField] private UpgradeData[] _prerequisites = null;

    // ── Public accessors ──────────────────────────────────────────────────

    public string UpgradeName   => _upgradeName;
    public Sprite Icon          => _icon;
    public string Description   => _description;
    public UpgradeCategory Category => _category;

    /// <summary>
    /// For Evolution upgrades: all listed upgrades must be in the player's slots
    /// before this evolution appears in the selection pool.
    /// </summary>
    public UpgradeData[] Prerequisites => _prerequisites;

    /// <summary>
    /// Returns the MonoBehaviour Type that implements this upgrade,
    /// or null if the type name is empty / not found.
    /// </summary>
    public System.Type GetBehaviourType()
    {
        if (string.IsNullOrEmpty(_behaviourTypeName)) return null;
        return System.Type.GetType(_behaviourTypeName);
    }
}
