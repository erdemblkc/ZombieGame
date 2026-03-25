/// <summary>
/// Rapid Fire: Ateş hızı +50% (fireCooldown × 0.67), hasar -%15 (damage × 0.85).
/// BehaviourTypeName: RapidFireUpgrade
/// </summary>
public class RapidFireUpgrade : GunUpgradeBase
{
    private const float DamageMul      = 0.85f;
    private const float CooldownMul    = 0.67f; // 50% more shots/sec → cooldown × 0.67

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
