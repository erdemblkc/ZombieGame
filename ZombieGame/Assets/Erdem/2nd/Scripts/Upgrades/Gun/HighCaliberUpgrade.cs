/// <summary>
/// High Caliber: Hasar +80% (damage × 1.80), ateş hızı -%40 (fireCooldown × 1.67).
/// BehaviourTypeName: HighCaliberUpgrade
/// </summary>
public class HighCaliberUpgrade : GunUpgradeBase
{
    private const float DamageMul   = 1.80f;
    private const float CooldownMul = 1.67f; // 40% fewer shots/sec → cooldown × 1.67

    public override void OnUpgradeEnabled(UnityEngine.GameObject player)
    {
        base.OnUpgradeEnabled(player);
        _modStack?.RegisterMultipliers(DamageMul, CooldownMul);
    }

    public override void OnUpgradeDisabled()
    {
        _modStack?.UnregisterMultipliers(DamageMul, CooldownMul);
        base.OnUpgradeDisabled();
    }
}
