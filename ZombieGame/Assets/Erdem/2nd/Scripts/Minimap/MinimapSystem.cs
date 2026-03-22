using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// MinimapCamera üzerine ekle.
/// Oyuncuyu takip eder, RenderTexture oluşturur ve ikonları günceller.
/// </summary>
[RequireComponent(typeof(Camera))]
public class MinimapSystem : MonoBehaviour
{
    public static MinimapSystem Instance { get; private set; }

    // ── Inspector ─────────────────────────────────────────────────────────────

    [Header("Camera Settings")]
    [Tooltip("Oyuncunun üzerindeki yükseklik (world unit) — zoom seviyesini belirler")]
    public float minimapHeight = 60f;

    [Header("References")]
    public Transform playerTransform;
    public RawImage  minimapDisplay;
    public RectTransform iconContainer;

    [Header("Icon Sprites (opsiyonel — Editor setup ile atanır)")]
    public Sprite playerArrowSprite;

    [Header("Display")]
    public float minimapSize = 200f;

    [Header("Enemy Display")]
    [Tooltip("Haritada düşman ikonları gösterilsin mi?")]
    public bool showEnemiesOnMap = false;

    [Header("Performance")]
    [Tooltip("Eş zamanlı gösterilecek maksimum ikon sayısı")]
    public int maxVisibleIcons = 50;

    // ── Private ───────────────────────────────────────────────────────────────

    private Camera           _cam;
    private RenderTexture    _rt;
    private readonly List<MinimapIcon> _icons = new();

    // ═════════════════════════════════════════════════════════════════════════

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        _cam = GetComponent<Camera>();
    }

    private void Start()
    {
        if (playerTransform == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTransform = p.transform;
        }

        InitCamera();
        InitRenderTexture();
    }

    private void InitCamera()
    {
        _cam.orthographic    = true;
        _cam.orthographicSize = minimapHeight * 0.5f;
        _cam.clearFlags      = CameraClearFlags.SolidColor;
        _cam.backgroundColor = Color.black;
        _cam.nearClipPlane   = 0.3f;
        _cam.farClipPlane    = minimapHeight + 50f;

        // Sabit yukarıdan bakış — kuzey daima yukarı
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        // UI layer'ı hariç tut (canvas elemanları minimap'e yansımasın)
        int uiLayer = LayerMask.NameToLayer("UI");
        if (uiLayer >= 0) _cam.cullingMask &= ~(1 << uiLayer);
    }

    private void InitRenderTexture()
    {
        int size = Mathf.RoundToInt(minimapSize);
        _rt = new RenderTexture(size, size, 16, RenderTextureFormat.ARGB32)
        {
            name         = "MinimapRT",
            filterMode   = FilterMode.Bilinear,
            antiAliasing = 1
        };

        _cam.targetTexture = _rt;

        if (minimapDisplay != null)
            minimapDisplay.texture = _rt;
    }

    // ── Update loop ───────────────────────────────────────────────────────────

    private void LateUpdate()
    {
        if (playerTransform == null) return;

        // Oyuncuyu takip et (X ve Z), sabit yükseklik
        Vector3 pos    = playerTransform.position;
        pos.y         += minimapHeight;
        transform.position = pos;

        RefreshIcons();
    }

    private void RefreshIcons()
    {
        if (iconContainer == null || playerTransform == null) return;

        var visible = BuildVisibleSet();

        foreach (var icon in _icons)
        {
            if (icon == null || icon.IconUI == null) continue;

            bool show = visible.Contains(icon)
                        && (icon.iconType != MinimapIcon.IconType.Enemy || showEnemiesOnMap);

            icon.IconUI.gameObject.SetActive(show);
            if (!show) continue;

            // Pozisyon güncelle
            icon.IconUI.anchoredPosition = WorldToMinimapPosition(icon.transform.position);

            // Sadece oyuncu ikonu döner; diğerleri kendi base rotasyonlarını korur
            if (icon.iconType == MinimapIcon.IconType.Player)
                icon.IconUI.localRotation = Quaternion.Euler(0f, 0f, -icon.transform.eulerAngles.y);
        }
    }

    private HashSet<MinimapIcon> BuildVisibleSet()
    {
        if (_icons.Count <= maxVisibleIcons)
            return new HashSet<MinimapIcon>(_icons);

        // 50'den fazla ikon varsa en yakın olanları göster
        Vector3 pp = playerTransform ? playerTransform.position : Vector3.zero;
        var sorted = new List<MinimapIcon>(_icons);
        sorted.Sort((a, b) =>
        {
            float da = a ? Vector3.SqrMagnitude(a.transform.position - pp) : float.MaxValue;
            float db = b ? Vector3.SqrMagnitude(b.transform.position - pp) : float.MaxValue;
            return da.CompareTo(db);
        });
        return new HashSet<MinimapIcon>(sorted.GetRange(0, maxVisibleIcons));
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// World pozisyonunu minimap UI pozisyonuna çevirir (piksel, merkez = 0,0).
    /// Diğer scriptler tarafından kullanılabilir.
    /// </summary>
    public Vector2 WorldToMinimapPosition(Vector3 worldPos)
    {
        if (playerTransform == null) return Vector2.zero;
        Vector3 offset = worldPos - playerTransform.position;
        float   scale  = minimapSize / minimapHeight;
        return new Vector2(offset.x * scale, offset.z * scale);
    }

    public void RegisterIcon(MinimapIcon icon)
    {
        if (!_icons.Contains(icon))
            _icons.Add(icon);
    }

    public void UnregisterIcon(MinimapIcon icon) => _icons.Remove(icon);

    // ── Cleanup ───────────────────────────────────────────────────────────────

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        if (_rt != null) { _rt.Release(); Destroy(_rt); }
    }

#if UNITY_EDITOR
    // Sahne görünümünde kamera alanını göster
    private void OnDrawGizmosSelected()
    {
        if (playerTransform == null) return;
        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.3f);
        float half = minimapHeight * 0.5f;
        Vector3 center = playerTransform.position;
        Gizmos.DrawWireCube(center, new Vector3(minimapHeight, 2f, minimapHeight));
    }
#endif
}
