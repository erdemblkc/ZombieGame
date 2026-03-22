using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Minimap'te görünmesi gereken her objeye ekle.
/// MinimapSystem'e otomatik register/unregister olur.
/// Obje yok edilince kendi UI ikonunu da temizler.
/// </summary>
public class MinimapIcon : MonoBehaviour
{
    public enum IconType
    {
        Player,    // Beyaz ok — yönü gösterir
        Ammo,      // Sarı nokta
        Health,    // Yeşil nokta
        Antidote,  // Mor nokta
        Quest,     // Sarı baklava/diamond + ünlem
        Enemy      // Kırmızı nokta (showEnemiesOnMap true ise görünür)
    }

    [Header("Ikon Tipi")]
    public IconType iconType = IconType.Ammo;

    /// <summary>Minimap üzerindeki RectTransform referansı.</summary>
    public RectTransform IconUI { get; private set; }

    // ── Tip başına renk & boyut tablosu ──────────────────────────────────────
    private static readonly Color[] TypeColor =
    {
        Color.white,                       // Player
        new Color(1.00f, 0.90f, 0.00f),    // Ammo    — sarı
        new Color(0.20f, 1.00f, 0.30f),    // Health  — yeşil
        new Color(0.75f, 0.20f, 1.00f),    // Antidote— mor
        new Color(1.00f, 0.85f, 0.00f),    // Quest   — parlak sarı
        new Color(1.00f, 0.25f, 0.25f),    // Enemy   — kırmızı
    };

    private static readonly Vector2[] TypeSize =
    {
        new Vector2(12f, 18f),   // Player (dikdörtgen ok)
        new Vector2(10f, 10f),   // Ammo
        new Vector2(10f, 10f),   // Health
        new Vector2(10f, 10f),   // Antidote
        new Vector2(14f, 14f),   // Quest
        new Vector2(10f, 10f),   // Enemy
    };

    // ═════════════════════════════════════════════════════════════════════════

    private void Start()
    {
        if (MinimapSystem.Instance == null)             return;
        if (MinimapSystem.Instance.iconContainer == null) return;

        CreateIconUI();
        MinimapSystem.Instance.RegisterIcon(this);
    }

    private void CreateIconUI()
    {
        int idx  = (int)iconType;
        var go   = new GameObject($"MI_{iconType}_{gameObject.name}");
        var rt   = go.AddComponent<RectTransform>();

        rt.SetParent(MinimapSystem.Instance.iconContainer, false);
        rt.anchorMin  = Vector2.one * 0.5f;
        rt.anchorMax  = Vector2.one * 0.5f;
        rt.pivot      = Vector2.one * 0.5f;
        rt.sizeDelta  = TypeSize[idx];

        var img   = go.AddComponent<Image>();
        img.color = TypeColor[idx];
        img.raycastTarget = false;

        // Oyuncu ikonu: varsa arrow sprite kullan
        if (iconType == IconType.Player && MinimapSystem.Instance.playerArrowSprite != null)
            img.sprite = MinimapSystem.Instance.playerArrowSprite;

        // Quest ikonu: 45° döndür → baklava/diamond görünümü
        if (iconType == IconType.Quest)
            rt.localRotation = Quaternion.Euler(0f, 0f, 45f);

        IconUI = rt;
    }

    private void OnDestroy()
    {
        MinimapSystem.Instance?.UnregisterIcon(this);
        if (IconUI != null) Destroy(IconUI.gameObject);
    }
}
