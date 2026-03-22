#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Tools → MainMenu → Create Main Menu Scene
/// MainMenu sahnesini sıfırdan oluşturur ve Build Settings'e ekler.
/// </summary>
public static class MainMenuSetup
{
    // ── Renkler ───────────────────────────────────────────────────────────────
    private static readonly Color C_BG         = new Color(0.07f, 0.07f, 0.12f);   // koyu lacivert
    private static readonly Color C_PANEL      = new Color(0.10f, 0.10f, 0.18f, 0.97f);
    private static readonly Color C_BTN        = new Color(0.16f, 0.16f, 0.28f);
    private static readonly Color C_BTN_HOVER  = new Color(0.26f, 0.26f, 0.48f);
    private static readonly Color C_BTN_PRESS  = new Color(0.10f, 0.10f, 0.20f);
    private static readonly Color C_ACCENT     = new Color(0.40f, 0.65f, 1.00f);   // açık mavi
    private static readonly Color C_WHITE      = Color.white;
    private static readonly Color C_SEPARATOR  = new Color(1f, 1f, 1f, 0.08f);

    private const string SCENE_PATH = "Assets/Erdem/2nd/Scenes/MainMenu.unity";

    // ═════════════════════════════════════════════════════════════════════════

    [MenuItem("Tools/MainMenu/Create Main Menu Scene")]
    public static void CreateScene()
    {
        if (EditorUtility.DisplayDialog("Main Menu",
            "MainMenu.unity oluşturulacak ve Build Settings güncellenecek.\nDevam edilsin mi?",
            "Evet", "İptal") == false) return;

        // Yeni boş sahne
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        BuildScene(scene);

        EditorSceneManager.SaveScene(scene, SCENE_PATH);
        UpdateBuildSettings();

        AssetDatabase.Refresh();
        Debug.Log("[MainMenuSetup] MainMenu.unity oluşturuldu ve Build Settings güncellendi.");
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Sahne inşası
    // ═════════════════════════════════════════════════════════════════════════

    private static void BuildScene(UnityEngine.SceneManagement.Scene scene)
    {
        // Kamera
        var camGO        = new GameObject("Main Camera");
        var cam          = camGO.AddComponent<Camera>();
        cam.clearFlags   = CameraClearFlags.SolidColor;
        cam.backgroundColor = C_BG;
        cam.orthographic = false;
        camGO.AddComponent<AudioListener>();

        // Event System
        var esGO = new GameObject("EventSystem");
        esGO.AddComponent<EventSystem>();
        esGO.AddComponent<StandaloneInputModule>();

        // Canvas
        var canvasGO   = new GameObject("Canvas");
        var canvas     = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;
        var scaler     = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight  = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        Transform cRoot = canvasGO.transform;

        // ── Arka plan ─────────────────────────────────────────────────────────
        var bg    = CreateRT("Background", cRoot);
        Stretch(bg);
        var bgImg = bg.gameObject.AddComponent<Image>();
        bgImg.color = C_BG;
        bgImg.raycastTarget = false;

        // ── Fade Overlay (sahne geçişi için) ──────────────────────────────────
        var fadeGO = CreateRT("FadeOverlay", cRoot);
        Stretch(fadeGO);
        var fadeImg  = fadeGO.gameObject.AddComponent<Image>();
        fadeImg.color = Color.black;
        fadeImg.raycastTarget = false;
        var fadeCG   = fadeGO.gameObject.AddComponent<CanvasGroup>();
        fadeCG.alpha = 1f;

        // ── Ana Panel (mainPanel) ──────────────────────────────────────────────
        var mainPanelGO = CreateRT("MainPanel", cRoot);
        Stretch(mainPanelGO);
        var mainCG = mainPanelGO.gameObject.AddComponent<CanvasGroup>();
        mainCG.alpha = 0f;

        Transform mp = mainPanelGO.transform;

        // Başlık
        var titleRT = CreateRT("TitleText", mp);
        titleRT.anchorMin = new Vector2(0.5f, 0.5f);
        titleRT.anchorMax = new Vector2(0.5f, 0.5f);
        titleRT.pivot     = new Vector2(0.5f, 0.5f);
        titleRT.sizeDelta = new Vector2(700f, 130f);
        titleRT.anchoredPosition = new Vector2(0f, 210f);
        var titleTmp = titleRT.gameObject.AddComponent<TextMeshProUGUI>();
        titleTmp.text      = "ZOMBIE GAME";
        titleTmp.fontSize  = 72f;
        titleTmp.fontStyle = FontStyles.Bold;
        titleTmp.color     = C_WHITE;
        titleTmp.alignment = TextAlignmentOptions.Center;

        // Alt başlık
        var subRT  = CreateRT("SubtitleText", mp);
        subRT.anchorMin = new Vector2(0.5f, 0.5f);
        subRT.anchorMax = new Vector2(0.5f, 0.5f);
        subRT.pivot     = new Vector2(0.5f, 0.5f);
        subRT.sizeDelta = new Vector2(500f, 40f);
        subRT.anchoredPosition = new Vector2(0f, 145f);
        var subTmp = subRT.gameObject.AddComponent<TextMeshProUGUI>();
        subTmp.text      = "Hayatta Kal";
        subTmp.fontSize  = 22f;
        subTmp.color     = C_ACCENT;
        subTmp.alignment = TextAlignmentOptions.Center;

        // Ayırıcı çizgi
        var line = CreateRT("Separator", mp);
        line.anchorMin = new Vector2(0.5f, 0.5f);
        line.anchorMax = new Vector2(0.5f, 0.5f);
        line.sizeDelta = new Vector2(300f, 1f);
        line.anchoredPosition = new Vector2(0f, 112f);
        var lineImg = line.gameObject.AddComponent<Image>();
        lineImg.color = C_SEPARATOR;
        lineImg.raycastTarget = false;

        // Buton container
        var btnContainer = CreateRT("ButtonContainer", mp);
        btnContainer.anchorMin = new Vector2(0.5f, 0.5f);
        btnContainer.anchorMax = new Vector2(0.5f, 0.5f);
        btnContainer.pivot     = new Vector2(0.5f, 0.5f);
        btnContainer.sizeDelta = new Vector2(260f, 230f);
        btnContainer.anchoredPosition = new Vector2(0f, -30f);

        var vLayout = btnContainer.gameObject.AddComponent<VerticalLayoutGroup>();
        vLayout.spacing            = 14f;
        vLayout.childAlignment     = TextAnchor.MiddleCenter;
        vLayout.childControlWidth  = true;
        vLayout.childControlHeight = false;
        vLayout.childForceExpandWidth  = true;
        vLayout.childForceExpandHeight = false;
        vLayout.padding = new RectOffset(0, 0, 0, 0);

        var startBtn   = MakeButton("StartButton",   "BAŞLAT",   btnContainer.transform, C_ACCENT, C_BG);
        var optionsBtn = MakeButton("OptionsButton", "AYARLAR",  btnContainer.transform, C_BTN,    C_BTN_HOVER);
        var quitBtn    = MakeButton("QuitButton",    "ÇIKIŞ",    btnContainer.transform, C_BTN,    C_BTN_HOVER);

        // Ses toggle butonu (sol alt)
        var soundBtnRT  = CreateRT("SoundToggleButton", mp);
        soundBtnRT.anchorMin = Vector2.zero;
        soundBtnRT.anchorMax = Vector2.zero;
        soundBtnRT.pivot     = Vector2.zero;
        soundBtnRT.sizeDelta = new Vector2(52f, 52f);
        soundBtnRT.anchoredPosition = new Vector2(30f, 30f);
        var soundBtn     = soundBtnRT.gameObject.AddComponent<Button>();
        StyleButton(soundBtn, C_BTN, C_BTN_HOVER, C_BTN_PRESS);
        var soundBtnImg  = soundBtnRT.gameObject.AddComponent<Image>();
        soundBtnImg.color = C_BTN;

        var soundIconRT  = CreateRT("SoundIcon", soundBtnRT.transform);
        Stretch(soundIconRT);
        soundIconRT.offsetMin = new Vector2(10f,  10f);
        soundIconRT.offsetMax = new Vector2(-10f, -10f);
        var soundIconImg = soundIconRT.gameObject.AddComponent<Image>();
        soundIconImg.color = C_WHITE;

        var soundLabelRT = CreateRT("SoundLabel", soundBtnRT.transform);
        soundLabelRT.anchorMin = new Vector2(1f, 0f);
        soundLabelRT.anchorMax = new Vector2(1f, 1f);
        soundLabelRT.pivot     = new Vector2(0f, 0.5f);
        soundLabelRT.sizeDelta = new Vector2(60f, 0f);
        soundLabelRT.anchoredPosition = new Vector2(6f, 0f);
        var soundLabelTmp = soundLabelRT.gameObject.AddComponent<TextMeshProUGUI>();
        soundLabelTmp.text      = "SES";
        soundLabelTmp.fontSize  = 13f;
        soundLabelTmp.color     = new Color(1f, 1f, 1f, 0.6f);
        soundLabelTmp.alignment = TextAlignmentOptions.MidlineLeft;

        // ── Options Paneli ────────────────────────────────────────────────────
        var optPanelRT = CreateRT("OptionsPanel", cRoot);
        optPanelRT.anchorMin = new Vector2(0.5f, 0.5f);
        optPanelRT.anchorMax = new Vector2(0.5f, 0.5f);
        optPanelRT.pivot     = new Vector2(0.5f, 0.5f);
        optPanelRT.sizeDelta = new Vector2(480f, 360f);
        optPanelRT.anchoredPosition = Vector2.zero;

        var optPanelImg  = optPanelRT.gameObject.AddComponent<Image>();
        optPanelImg.color = C_PANEL;
        var optCG = optPanelRT.gameObject.AddComponent<CanvasGroup>();
        optCG.alpha = 0f;
        optCG.blocksRaycasts = false;

        Transform optT = optPanelRT.transform;

        // Options başlık
        var optTitleRT  = CreateRT("OptionsTitle", optT);
        optTitleRT.anchorMin = new Vector2(0f, 1f);
        optTitleRT.anchorMax = new Vector2(1f, 1f);
        optTitleRT.pivot     = new Vector2(0.5f, 1f);
        optTitleRT.sizeDelta = new Vector2(0f, 60f);
        optTitleRT.anchoredPosition = new Vector2(0f, 0f);
        var optTitleTmp  = optTitleRT.gameObject.AddComponent<TextMeshProUGUI>();
        optTitleTmp.text      = "AYARLAR";
        optTitleTmp.fontSize  = 26f;
        optTitleTmp.fontStyle = FontStyles.Bold;
        optTitleTmp.color     = C_WHITE;
        optTitleTmp.alignment = TextAlignmentOptions.Center;

        // Volume label
        var volLabelRT  = CreateRT("VolumeLabel", optT);
        volLabelRT.anchorMin = new Vector2(0f, 0.5f);
        volLabelRT.anchorMax = new Vector2(1f, 0.5f);
        volLabelRT.pivot     = new Vector2(0.5f, 0.5f);
        volLabelRT.sizeDelta = new Vector2(-60f, 36f);
        volLabelRT.anchoredPosition = new Vector2(0f, 60f);
        var volLabelTmp  = volLabelRT.gameObject.AddComponent<TextMeshProUGUI>();
        volLabelTmp.text      = "Ana Ses";
        volLabelTmp.fontSize  = 18f;
        volLabelTmp.color     = new Color(1f, 1f, 1f, 0.75f);
        volLabelTmp.alignment = TextAlignmentOptions.Left;

        // Volume value text
        var volValueRT  = CreateRT("VolumeValue", optT);
        volValueRT.anchorMin = new Vector2(1f, 0.5f);
        volValueRT.anchorMax = new Vector2(1f, 0.5f);
        volValueRT.pivot     = new Vector2(1f, 0.5f);
        volValueRT.sizeDelta = new Vector2(60f, 36f);
        volValueRT.anchoredPosition = new Vector2(-20f, 60f);
        var volValueTmp  = volValueRT.gameObject.AddComponent<TextMeshProUGUI>();
        volValueTmp.text      = "100%";
        volValueTmp.fontSize  = 16f;
        volValueTmp.color     = C_ACCENT;
        volValueTmp.alignment = TextAlignmentOptions.Right;

        // Volume slider
        var sliderRT = CreateRT("MasterVolumeSlider", optT);
        sliderRT.anchorMin = new Vector2(0f, 0.5f);
        sliderRT.anchorMax = new Vector2(1f, 0.5f);
        sliderRT.pivot     = new Vector2(0.5f, 0.5f);
        sliderRT.sizeDelta = new Vector2(-60f, 28f);
        sliderRT.anchoredPosition = new Vector2(0f, 10f);
        var slider = sliderRT.gameObject.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value    = 1f;
        // Slider bg
        var slBgRT  = CreateRT("Background", sliderRT.transform);
        Stretch(slBgRT);
        var slBgImg = slBgRT.gameObject.AddComponent<Image>();
        slBgImg.color = new Color(0.2f, 0.2f, 0.35f);
        // Fill area
        var fillAreaRT = CreateRT("Fill Area", sliderRT.transform);
        fillAreaRT.anchorMin = new Vector2(0f, 0.25f);
        fillAreaRT.anchorMax = new Vector2(1f, 0.75f);
        fillAreaRT.offsetMin = new Vector2(5f, 0f);
        fillAreaRT.offsetMax = new Vector2(-5f, 0f);
        var fillRT  = CreateRT("Fill", fillAreaRT.transform);
        Stretch(fillRT);
        var fillImg = fillRT.gameObject.AddComponent<Image>();
        fillImg.color = C_ACCENT;
        slider.fillRect = fillRT;
        // Handle
        var handleAreaRT = CreateRT("Handle Slide Area", sliderRT.transform);
        Stretch(handleAreaRT);
        var handleRT  = CreateRT("Handle", handleAreaRT.transform);
        handleRT.sizeDelta = new Vector2(20f, 20f);
        var handleImg = handleRT.gameObject.AddComponent<Image>();
        handleImg.color = C_WHITE;
        slider.handleRect     = handleRT;
        slider.targetGraphic  = handleImg;

        // Kapat butonu
        var closeBtnRT = CreateRT("CloseButton", optT);
        closeBtnRT.anchorMin = new Vector2(0.5f, 0f);
        closeBtnRT.anchorMax = new Vector2(0.5f, 0f);
        closeBtnRT.pivot     = new Vector2(0.5f, 0f);
        closeBtnRT.sizeDelta = new Vector2(180f, 46f);
        closeBtnRT.anchoredPosition = new Vector2(0f, 28f);
        var closeBtn = MakeButton("CloseButton", "KAPAT", optT, C_BTN, C_BTN_HOVER);
        // Override position (MakeButton adds LayoutElement which we don't want here)
        var closeRT = closeBtn.GetComponent<RectTransform>();
        closeRT.anchorMin = new Vector2(0.5f, 0f);
        closeRT.anchorMax = new Vector2(0.5f, 0f);
        closeRT.pivot     = new Vector2(0.5f, 0f);
        closeRT.sizeDelta = new Vector2(180f, 46f);
        closeRT.anchoredPosition = new Vector2(0f, 28f);

        // ── MainMenuManager bağlantıları ──────────────────────────────────────
        var mgrGO  = new GameObject("MainMenuManager");
        var mgr    = mgrGO.AddComponent<MainMenuManager>();

        mgr.gameSceneName     = "GameScene";
        mgr.mainPanel         = mainCG;
        mgr.fadeOverlay       = fadeCG;
        mgr.optionsPanel      = optPanelRT;
        mgr.optionsCG         = optCG;
        mgr.startButton       = startBtn;
        mgr.optionsButton     = optionsBtn;
        mgr.quitButton        = quitBtn;
        mgr.soundToggleButton = soundBtn;
        mgr.optionsCloseButton = closeBtn;
        mgr.soundIconImage    = soundIconImg;
        mgr.masterVolumeSlider = slider;
        mgr.volumeValueText   = volValueTmp;

        EditorUtility.SetDirty(mgrGO);
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Build Settings güncelle
    // ═════════════════════════════════════════════════════════════════════════

    private static void UpdateBuildSettings()
    {
        var scenes = new[]
        {
            new EditorBuildSettingsScene(SCENE_PATH,                                   true),
            new EditorBuildSettingsScene("Assets/Erdem/2nd/Scenes/GameScene.unity",    true),
        };
        EditorBuildSettings.scenes = scenes;
        Debug.Log("[MainMenuSetup] Build Settings güncellendi: MainMenu(0) → GameScene(1)");
    }

    // ═════════════════════════════════════════════════════════════════════════
    // UI Yardımcıları
    // ═════════════════════════════════════════════════════════════════════════

    private static Button MakeButton(string name, string label, Transform parent,
                                     Color normalColor, Color hoverColor)
    {
        var rt      = CreateRT(name, parent);
        rt.sizeDelta = new Vector2(0f, 54f);

        var le = rt.gameObject.AddComponent<LayoutElement>();
        le.preferredHeight = 54f;

        var bgImg   = rt.gameObject.AddComponent<Image>();
        bgImg.color = normalColor;

        var btn     = rt.gameObject.AddComponent<Button>();
        StyleButton(btn, normalColor, hoverColor, C_BTN_PRESS);
        btn.targetGraphic = bgImg;

        var txtRT   = CreateRT("Label", rt.transform);
        Stretch(txtRT);
        var tmp     = txtRT.gameObject.AddComponent<TextMeshProUGUI>();
        tmp.text      = label;
        tmp.fontSize  = 18f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color     = C_WHITE;
        tmp.alignment = TextAlignmentOptions.Center;

        return btn;
    }

    private static void StyleButton(Button btn, Color normal, Color hover, Color pressed)
    {
        var cb          = btn.colors;
        cb.normalColor      = normal;
        cb.highlightedColor = hover;
        cb.pressedColor     = pressed;
        cb.selectedColor    = normal;
        cb.fadeDuration     = 0.1f;
        btn.colors          = cb;
    }

    private static RectTransform CreateRT(string name, Transform parent)
    {
        var go = new GameObject(name);
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
}
#endif
