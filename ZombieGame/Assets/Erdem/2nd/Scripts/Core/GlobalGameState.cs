using UnityEngine;

/// <summary>
/// Sahneler arası kalıcı veri deposu.
/// Run verileri (runCurrency, floor) run başında sıfırlanır.
/// Meta veriler (metaCurrency) hiç sıfırlanmaz — PlayerPrefs ile kalıcı.
/// </summary>
public static class GlobalGameState
{
    // ── Eski alanlar (geriye uyumluluk) ─────────────────────────────────
    public static int  SavedWave         = 1;
    public static bool IsWeaponUpgraded  = false;

    // ── Run State (her run başında sıfırlanır) ───────────────────────────
    /// <summary>Şu anki bölüm (1 = Hastane, 2 = Metro, 3 = Laboratuvar)</summary>
    public static int  CurrentFloor      = 1;
    /// <summary>Bölüm içindeki oda sırası (0'dan başlar)</summary>
    public static int  CurrentRoomIndex  = 0;
    /// <summary>Run içinde biriken para — bölüm sonu hariç sıfırlanmaz</summary>
    public static int  RunCurrency       = 0;
    /// <summary>Seçilen oda tipi (RoomManager tarafından set edilir)</summary>
    public static RoomType NextRoomType  = RoomType.Combat;

    // ── Meta State (kalıcı, PlayerPrefs'te) ─────────────────────────────
    /// <summary>Run'lar arası biriken Serum — kalıcı upgrade için harcanır</summary>
    public static int MetaCurrency
    {
        get => PlayerPrefs.GetInt("MetaCurrency", 0);
        set { PlayerPrefs.SetInt("MetaCurrency", value); PlayerPrefs.Save(); }
    }

    // ── Permanent Bonuses (PermanentUpgradeStation tarafından set edilir) ──
    /// <summary>Kalıcı max HP bonusu (flat)</summary>
    public static int   PermanentMaxHPBonus           = 0;
    /// <summary>Run başında HP yüzdesi bonusu (0.1 = +%10)</summary>
    public static float PermanentStartingHPBonus      = 0f;
    /// <summary>Kalıcı hasar çarpanı bonusu (0.1 = +%10)</summary>
    public static float PermanentDamageBonus          = 0f;
    /// <summary>Kalıcı enfeksiyon yavaşlama bonusu (0.1 = %10 daha yavaş artar)</summary>
    public static float PermanentInfectionSlowBonus   = 0f;
    /// <summary>Upgrade seçiminde ek kart sayısı</summary>
    public static int   PermanentExtraUpgradeChoices  = 0;
    /// <summary>Run sonu Serum dönüşüm bonusu (0.1 = +%10)</summary>
    public static float PermanentSerumBonus           = 0f;

    /// <summary>Kalıcı bonusları sıfırla — PermanentUpgradeStation.ResetAll() çağırır.</summary>
    public static void ResetPermanentBonuses()
    {
        PermanentMaxHPBonus          = 0;
        PermanentStartingHPBonus     = 0f;
        PermanentDamageBonus         = 0f;
        PermanentInfectionSlowBonus  = 0f;
        PermanentExtraUpgradeChoices = 0;
        PermanentSerumBonus          = 0f;
    }

    // ── Helpers ─────────────────────────────────────────────────────────

    /// <summary>Yeni run başlarken çağır — run state sıfırlanır, meta korunur.</summary>
    public static void StartNewRun()
    {
        SavedWave        = 1;
        IsWeaponUpgraded = false;
        CurrentFloor     = 1;
        CurrentRoomIndex = 0;
        RunCurrency      = 0;
        NextRoomType     = RoomType.Combat;
    }

    /// <summary>Oyunu tamamen sıfırla (ana menüden başlarken).</summary>
    public static void ResetData()
    {
        SavedWave        = 1;
        IsWeaponUpgraded = false;
        CurrentFloor     = 1;
        CurrentRoomIndex = 0;
        RunCurrency      = 0;
        NextRoomType     = RoomType.Combat;
        // MetaCurrency kasıtlı olarak sıfırlanmıyor
    }
}

/// <summary>Oda tipleri — RoomManager ve RoomSelectionUI tarafından kullanılır.</summary>
public enum RoomType
{
    Combat,   // Normal zombi odası
    Elite,    // Güçlü elite zombi, ekstra ödül
    Boss,     // Bölüm sonu boss
    Shop,     // Run currency ile alım
    Rest,     // HP veya Enfeksiyon seçimi
    Event,    // Risk/ödül etkinliği
}
