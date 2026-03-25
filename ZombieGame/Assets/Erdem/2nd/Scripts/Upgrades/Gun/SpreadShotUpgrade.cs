/// <summary>
/// Spread Shot: Her ateşte koni içinde 2 ekstra mermi çıkar (70% hasar, ±15° yayılım).
/// GunShooter, GunModifierStack.HasSpreadShot flag'ini kontrol ederek ekstra mermileri
/// kendi ShootProjectile() içinde spawn eder.
/// BehaviourTypeName: SpreadShotUpgrade
/// </summary>
public class SpreadShotUpgrade : GunUpgradeBase
{
    public override void OnUpgradeEnabled(UnityEngine.GameObject player)
    {
        base.OnUpgradeEnabled(player);
        _modStack?.SetSpreadShot(true);
    }

    public override void OnUpgradeDisabled()
    {
        _modStack?.SetSpreadShot(false);
        base.OnUpgradeDisabled();
    }
}
