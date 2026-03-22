#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Menü: Tools → Minimap → Setup Minimap
/// Sahnede tam minimap hiyerarşisini + texture asset'lerini otomatik oluşturur.
/// </summary>
public static class MinimapSetup
{
    private const string AssetFolder  = "Assets/Erdem/2nd/Minimap";
    private const int    TexSize      = 128;
    private const int    RT_SIZE      = 200;
    private const float  RingThickness = 5f;   // piksel

    // ═════════════════════════════════════════════════════════════════════════

    [MenuItem("Tools/Minimap/Setup Minimap")]
    public static void Setup()
    {
        if (Object.FindObjectOfType<MinimapSystem>() != null)
        {
            EditorUtility.DisplayDialog("Minimap", "Sahnede zaten bir MinimapSystem var.", "Tamam");
            return;
        }

        EnsureFolder();

        var rt          = GetOrCreateRenderTexture();
        var circleSprite = GetOrCreateSprite("MinimapCircle", GenerateCircleTex);
        var ringSprite   = GetOrCreateSprite("MinimapRing",   GenerateRingTex);
        var arrowSprite  = GetOrCreateSprite("MinimapArrow",  GenerateArrowTex);

        // ── MinimapCamera ────────────────────────────────────────────────────
        var camGO = new GameObject("MinimapCamera");
        Undo.RegisterCreatedObjectUndo(camGO, "Create MinimapCamera");

        var cam               = camGO.AddComponent<Camera>();
        cam.orthographic      = true;
        cam.orthographicSize  = 30f;
        cam.clearFlags        = CameraClearFlags.SolidColor;
        cam.backgroundColor   = new Color(0.05f, 0.05f, 0.05f, 1f);
        cam.targetTexture     = rt;
        cam.nearClipPlane     = 0.3f;
        cam.farClipPlane      = 120f;
        cam.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        cam.transform.position = new Vector3(0f, 60f, 0f);

        int uiLayer = LayerMask.NameToLayer("UI");
        if (uiLayer >= 0) cam.cullingMask &= ~(1 << uiLayer);

        var sys                  = camGO.AddComponent<MinimapSystem>();
        sys.minimapHeight        = 60f;
        sys.minimapSize          = RT_SIZE;
        sys.playerArrowSprite    = arrowSprite;

        // ── Canvas ───────────────────────────────────────────────────────────
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            var cgo = new GameObject("Canvas");
            Undo.RegisterCreatedObjectUndo(cgo, "Create Canvas");
            canvas = cgo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = cgo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            cgo.AddComponent<GraphicRaycaster>();
        }

        // ── MinimapContainer (sol alt, 20px margin) ──────────────────────────
        var container        = CreateRT("MinimapContainer", canvas.transform);
        container.anchorMin  = Vector2.zero;
        container.anchorMax  = Vector2.zero;
        container.pivot      = Vector2.zero;
        container.sizeDelta  = new Vector2(RT_SIZE, RT_SIZE);
        container.anchoredPosition = new Vector2(20f, 20f);

        // ── Background: siyah daire, %70 opacity ────────────────────────────
        var bg    = CreateRT("Background", container);
        Stretch(bg);
        var bgImg  = bg.gameObject.AddComponent<Image>();
        bgImg.sprite = circleSprite;
        bgImg.color  = new Color(0f, 0f, 0f, 0.7f);
        bgImg.type   = Image.Type.Simple;
        bgImg.raycastTarget = false;

        // ── CircleMask: kamera görüntüsünü daire şeklinde kırp ───────────────
        var maskRT = CreateRT("CircleMask", container);
        Stretch(maskRT);
        var maskImg  = maskRT.gameObject.AddComponent<Image>();
        maskImg.sprite = circleSprite;
        maskImg.color  = Color.white;
        maskImg.raycastTarget = false;
        var mask = maskRT.gameObject.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        //   MinimapRawImage — RenderTexture'ı gösterir (CircleMask içinde)
        var rawRT  = CreateRT("MinimapRawImage", maskRT);
        Stretch(rawRT);
        var rawImg = rawRT.gameObject.AddComponent<RawImage>();
        rawImg.texture       = rt;
        rawImg.raycastTarget = false;

        //   IconContainer — marker'lar buraya spawn olur (CircleMask içinde)
        var iconContainer = CreateRT("IconContainer", maskRT);
        Stretch(iconContainer);

        // ── Border: beyaz halka, %60 opacity ─────────────────────────────────
        var border    = CreateRT("Border", container);
        Stretch(border);
        var borderImg  = border.gameObject.AddComponent<Image>();
        borderImg.sprite = ringSprite;
        borderImg.color  = new Color(1f, 1f, 1f, 0.6f);
        borderImg.type   = Image.Type.Simple;
        borderImg.raycastTarget = false;

        // ── CompassLabel: sağ üst köşede "K" ─────────────────────────────────
        var compassRT        = CreateRT("CompassLabel", container);
        compassRT.anchorMin  = Vector2.one;
        compassRT.anchorMax  = Vector2.one;
        compassRT.pivot      = Vector2.one;
        compassRT.sizeDelta  = new Vector2(22f, 22f);
        compassRT.anchoredPosition = new Vector2(-6f, -6f);

        var tmp            = compassRT.gameObject.AddComponent<TextMeshProUGUI>();
        tmp.text           = "K";
        tmp.fontSize       = 11f;
        tmp.fontStyle      = FontStyles.Bold;
        tmp.color          = Color.white;
        tmp.alignment      = TextAlignmentOptions.Center;

        // ── MinimapSystem referanslarını bağla ────────────────────────────────
        sys.minimapDisplay = rawImg;
        sys.iconContainer  = iconContainer;

        // ── Kaydet & seç ──────────────────────────────────────────────────────
        EditorUtility.SetDirty(camGO);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Selection.activeGameObject = camGO;

        Debug.Log("[MinimapSetup] Tamamlandı! MinimapSystem Inspector'ında <b>playerTransform</b>'u ata.");
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Texture üretici metotlar
    // ═════════════════════════════════════════════════════════════════════════

    private static Texture2D GenerateCircleTex()
    {
        var tex = new Texture2D(TexSize, TexSize, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        float c  = TexSize * 0.5f;
        float r  = c - 0.5f;
        for (int y = 0; y < TexSize; y++)
            for (int x = 0; x < TexSize; x++)
            {
                float d     = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(c, c));
                float alpha = Mathf.Clamp01((r - d) / 1.5f);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        tex.Apply();
        return tex;
    }

    private static Texture2D GenerateRingTex()
    {
        var tex = new Texture2D(TexSize, TexSize, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        float c      = TexSize * 0.5f;
        float outer  = c - 0.5f;
        float inner  = outer - RingThickness;
        for (int y = 0; y < TexSize; y++)
            for (int x = 0; x < TexSize; x++)
            {
                float d         = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(c, c));
                float oAlpha    = Mathf.Clamp01((outer - d)  / 1.5f);
                float iAlpha    = Mathf.Clamp01((d - inner)  / 1.5f);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, oAlpha * iAlpha));
            }
        tex.Apply();
        return tex;
    }

    /// <summary>
    /// Yukarı bakan üçgen ok — oyuncu yönünü gösterir.
    /// Tip: üst merkez, taban: alt.
    /// </summary>
    private static Texture2D GenerateArrowTex()
    {
        const int S = 32;
        var tex = new Texture2D(S, S, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        float cx = S * 0.5f;

        for (int y = 0; y < S; y++)
            for (int x = 0; x < S; x++)
            {
                // normX: -1..1, normY: 0(alt)..1(üst)
                float nx = (x + 0.5f - cx) / cx;
                float ny = (y + 0.5f) / S;

                // Üçgen: tip yukarıda → |nx| <= (1 - ny)
                bool inside = Mathf.Abs(nx) <= (1f - ny) - 0.05f;
                tex.SetPixel(x, y, inside ? Color.white : Color.clear);
            }
        tex.Apply();
        return tex;
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Asset yardımcıları
    // ═════════════════════════════════════════════════════════════════════════

    private delegate Texture2D TexFactory();

    private static Sprite GetOrCreateSprite(string name, TexFactory factory)
    {
        string pngPath = $"{AssetFolder}/{name}.png";
        string absPath = Application.dataPath + "/" + pngPath.Substring("Assets/".Length);

        // Var olan asset'i tekrar kullan
        if (File.Exists(absPath))
        {
            var existing = AssetDatabase.LoadAssetAtPath<Sprite>(pngPath);
            if (existing != null) return existing;
        }

        // Üret ve kaydet
        Texture2D tex = factory();
        File.WriteAllBytes(absPath, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);

        AssetDatabase.ImportAsset(pngPath);

        var imp = (TextureImporter)AssetImporter.GetAtPath(pngPath);
        imp.textureType        = TextureImporterType.Sprite;
        imp.spriteImportMode   = SpriteImportMode.Single;
        imp.alphaIsTransparency = true;
        imp.filterMode         = FilterMode.Bilinear;
        imp.mipmapEnabled      = false;
        imp.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Sprite>(pngPath);
    }

    private static RenderTexture GetOrCreateRenderTexture()
    {
        string path = $"{AssetFolder}/MinimapRenderTexture.renderTexture";
        var existing = AssetDatabase.LoadAssetAtPath<RenderTexture>(path);
        if (existing != null) return existing;

        var rt = new RenderTexture(RT_SIZE, RT_SIZE, 16, RenderTextureFormat.ARGB32)
        {
            name         = "MinimapRenderTexture",
            filterMode   = FilterMode.Bilinear,
            antiAliasing = 1
        };
        AssetDatabase.CreateAsset(rt, path);
        AssetDatabase.SaveAssets();
        return rt;
    }

    private static void EnsureFolder()
    {
        if (!AssetDatabase.IsValidFolder(AssetFolder))
        {
            AssetDatabase.CreateFolder("Assets/Erdem/2nd", "Minimap");
            AssetDatabase.Refresh();
        }
    }

    // ── UI yardımcıları ───────────────────────────────────────────────────────

    private static RectTransform CreateRT(string name, Transform parent)
    {
        var go = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(go, $"Create {name}");
        go.transform.SetParent(parent, false);
        return go.AddComponent<RectTransform>();
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin  = Vector2.zero;
        rt.anchorMax  = Vector2.one;
        rt.offsetMin  = Vector2.zero;
        rt.offsetMax  = Vector2.zero;
    }

    // ── Temizleme (test/geliştirme için) ─────────────────────────────────────

    [MenuItem("Tools/Minimap/Remove Minimap")]
    private static void Remove()
    {
        var sys = Object.FindObjectOfType<MinimapSystem>();
        if (sys != null)
        {
            Undo.DestroyObjectImmediate(sys.gameObject);

            // Canvas'ta MinimapContainer'ı bul ve sil
            var canvas = Object.FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                var container = canvas.transform.Find("MinimapContainer");
                if (container != null) Undo.DestroyObjectImmediate(container.gameObject);
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("[MinimapSetup] Minimap kaldırıldı.");
        }
        else
        {
            Debug.Log("[MinimapSetup] Sahnede MinimapSystem bulunamadı.");
        }
    }
}
#endif
