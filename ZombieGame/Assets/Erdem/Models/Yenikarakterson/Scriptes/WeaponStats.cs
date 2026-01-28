using UnityEngine;

public class WeaponStats : MonoBehaviour
{
    [Header("Firing Point (ÖNEMLÝ)")]
    // Her silahýn kendi namlu ucunu buraya atayacaðýz
    public Transform muzzle;

    [Header("Damage & Fire")]
    public float damage = 25f;
    public float fireCooldown = 0.12f;

    [Header("Ammo")]
    public int magazineSize = 12;
    public int reserveStart = 36;
    public float reloadTime = 1.0f;
    public bool autoReloadWhenEmpty = true;

    [Header("Projectile Mode")]
    public bool useDoubleBullet = false;
    public Bullet singleBulletPrefab;
    public DoubleBullet doubleBulletPrefab;

    [Header("Optional barrels (for DoubleBullet)")]
    public Transform leftBarrel;
    public Transform rightBarrel;
}