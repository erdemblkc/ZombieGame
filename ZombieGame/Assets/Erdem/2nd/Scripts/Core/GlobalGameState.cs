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
