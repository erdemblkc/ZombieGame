using UnityEngine;

/// <summary>
/// Upgrade nadirlık seviyesi — kart rengini ve drop ağırlığını belirler.
/// </summary>
public enum UpgradeRarity
{
    Common    = 0,  // Gri   — her zaman çıkar
    Uncommon  = 1,  // Yeşil — sık çıkar
    Rare      = 2,  // Mavi  — ara sıra çıkar
    Epic      = 3,  // Mor   — nadir, güçlü
}

/// <summary>
/// Rarity yardımcı sınıfı — renk ve ağırlık tablosu.
/// </summary>
public static class UpgradeRarityUtils
{
    // ── Renkler ───────────────────────────────────────────────────────────

    public static Color GetColor(UpgradeRarity rarity) => rarity switch
    {
        UpgradeRarity.Common   => new Color(0.75f, 0.75f, 0.75f), // gri
        UpgradeRarity.Uncommon => new Color(0.30f, 0.85f, 0.30f), // yeşil
        UpgradeRarity.Rare     => new Color(0.30f, 0.55f, 1.00f), // mavi
        UpgradeRarity.Epic     => new Color(0.70f, 0.20f, 1.00f), // mor
        _                      => Color.white,
    };

    // ── Display İsimleri ──────────────────────────────────────────────────

    public static string GetDisplayName(UpgradeRarity rarity) => rarity switch
    {
        UpgradeRarity.Common   => "COMMON",
        UpgradeRarity.Uncommon => "UNCOMMON",
        UpgradeRarity.Rare     => "RARE",
        UpgradeRarity.Epic     => "EPIC",
        _                      => "",
    };

    /// <summary>
    /// Ağırlıklı rastgele rarity seç.
    /// Ağırlıklar: Common=60, Uncommon=25, Rare=12, Epic=3
    /// </summary>
    public static UpgradeRarity RollRarity(float epicBonusChance = 0f)
    {
        float roll = Random.value * 100f;

        float epicThreshold     = 3f  + epicBonusChance * 100f;
        float rareThreshold     = 15f;
        float uncommonThreshold = 40f;

        if (roll < epicThreshold)     return UpgradeRarity.Epic;
        if (roll < rareThreshold)     return UpgradeRarity.Rare;
        if (roll < uncommonThreshold) return UpgradeRarity.Uncommon;
        return UpgradeRarity.Common;
    }
}
