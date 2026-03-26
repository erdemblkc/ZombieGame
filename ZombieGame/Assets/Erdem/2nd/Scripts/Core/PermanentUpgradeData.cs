using UnityEngine;

/// <summary>
/// Kalıcı upgrade tanımı — HUB'da Serum ile satın alınır, run'lar arası kalıcıdır.
/// PlayerPrefs'e kaydedilir.
/// </summary>
[CreateAssetMenu(fileName = "NewPermanentUpgrade", menuName = "Upgrades/PermanentUpgradeData")]
public class PermanentUpgradeData : ScriptableObject
{
    [Header("Kimlik")]
    [Tooltip("Benzersiz ID — PlayerPrefs key'i olarak kullanılır. Değiştirme!")]
    public string upgradeId;
    public string upgradeName;

    [TextArea(2, 3)]
    public string description;
    public Sprite icon;

    [Header("Maliyet")]
    public int serumCost = 20;

    [Header("Etki")]
    public PermanentUpgradeEffect effect;
    public float                  effectValue;

    [Header("Yükseltme Seviyeleri")]
    [Tooltip("Kaç kez satın alınabilir? 1 = tek seferlik")]
    public int maxLevel = 1;
}

public enum PermanentUpgradeEffect
{
    MaxHPBonus,           // Maksimum HP artışı (flat)
    StartingHPPercent,    // Run başında HP yüzdesi (0.1 = +%10)
    DamagePercent,        // Tüm hasar çarpanı
    InfectionSlowPercent, // Enfeksiyon artış hızı yavaşlatma
    StartWithUpgrade,     // Run başında belirli bir upgrade ile başla
    ExtraUpgradeChoice,   // Upgrade seçiminde +1 kart
    SerumBonusPercent,    // Run sonu Serum dönüşüm bonusu
}
