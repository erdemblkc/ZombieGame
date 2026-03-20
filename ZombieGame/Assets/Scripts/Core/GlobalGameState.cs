using UnityEngine;

public static class GlobalGameState
{
    // Oyun yeniden baþlayýnca hatýrlanacak veriler
    public static int SavedWave = 1;
    public static bool IsWeaponUpgraded = false;

    // Oyunu tamamen sýfýrlamak istersen bunu çaðýrýrýz (Örn: Ana menüden girince)
    public static void ResetData()
    {
        SavedWave = 1;
        IsWeaponUpgraded = false;
    }
}