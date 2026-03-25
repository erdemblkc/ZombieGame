/// <summary>
/// Ricochet: Mermi duvarlara çarpar ve yansır (max 2 sekme).
/// BehaviourTypeName: RicochetUpgrade
/// </summary>
public class RicochetUpgrade : GunUpgradeBase
{
    private const int BounceCount = 2;

    public override void OnUpgradeEnabled(UnityEngine.GameObject player)
    {
        base.OnUpgradeEnabled(player);
        _modStack?.AddRicochet(BounceCount);
    }

    public override void OnUpgradeDisabled()
    {
        _modStack?.AddRicochet(-BounceCount);
        base.OnUpgradeDisabled();
    }
}
