/// <summary>
/// Piercer: Mermi 3 düşmandan geçer. Bullet.piercingCount'u artırarak çalışır.
/// BehaviourTypeName: PiercerUpgrade
/// </summary>
public class PiercerUpgrade : GunUpgradeBase
{
    private const int PierceTargets = 3;

    public override void OnUpgradeEnabled(UnityEngine.GameObject player)
    {
        base.OnUpgradeEnabled(player);
        _modStack?.AddPiercing(PierceTargets);
    }

    public override void OnUpgradeDisabled()
    {
        _modStack?.AddPiercing(-PierceTargets);
        base.OnUpgradeDisabled();
    }
}
