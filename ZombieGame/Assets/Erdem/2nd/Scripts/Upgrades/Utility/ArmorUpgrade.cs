using UnityEngine;

/// <summary>
/// Armor: Gelen tüm hasar kalıcı olarak %15 azalır.
/// PlayerDamageReceiver.armorDamageMultiplier'ı 0.85'e düşürür.
/// BehaviourTypeName: ArmorUpgrade
/// </summary>
public class ArmorUpgrade : MonoBehaviour, IUpgrade
{
    private const float DamageReduction = 0.85f; // 15% less damage taken

    private UpgradeData _data;
    private bool _isActive;
    private bool _evolutionSuppressed;
    private PlayerDamageReceiver _player;

    public UpgradeData Data     => _data;
    public bool        IsActive => _isActive;

    public void OnUpgradeEnabled(GameObject player)
    {
        _player   = player.GetComponent<PlayerDamageReceiver>();
        _isActive = true;
        enabled   = true;

        if (_player != null)
            _player.armorDamageMultiplier *= DamageReduction;
    }

    public void OnUpgradeDisabled()
    {
        if (_player != null && DamageReduction != 0f)
            _player.armorDamageMultiplier /= DamageReduction;

        _isActive = false;
        enabled   = false;
    }

    public void SetEvolutionSuppressed(bool suppressed) => _evolutionSuppressed = suppressed;

    internal void SetData(UpgradeData data) => _data = data;

    void Awake() => enabled = false;
}
