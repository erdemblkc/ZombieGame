using UnityEngine;

public class WeaponStats : MonoBehaviour
{
    [Header("Firing Point")]
    public Transform muzzle;

    [Header("Audio (YENÝ)")]
    public AudioClip fireSound;   // Ateþ sesi dosyasýný buraya sürükle
    public AudioClip reloadSound; // Þarjör sesi dosyasýný buraya sürükle
    [Range(0, 1)] public float fireVolume = 0.8f;

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

    [Header("Optional barrels")]
    public Transform leftBarrel;
    public Transform rightBarrel;
}