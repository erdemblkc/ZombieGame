#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Unity menüsünden çalıştırılır:
///   Tools → ZombieGame → Setup All UI
///   Tools → ZombieGame → Setup Wave Manager
///   Tools → ZombieGame → Setup Player Components
///
/// Tüm UI hierarchy'i, component'leri ve iç referansları otomatik oluşturur/bağlar.
/// Mevcut objeler varsa yeniden oluşturmaz — güvenle tekrar çalıştırılabilir.
/// </summary>
public static class ZombieGameSetup
{
    // ── Ana Menü ─────────────────────────────────────────────────────────

    [MenuItem("Tools/ZombieGame/▶ Setup ALL (UI + WaveManager + Player + Assets)", false, 1)]
    static void SetupAll()
    {
        SetupUI();
        SetupWaveManager();
        SetupPlayerComponents();
        CreateAllUpgradeAssets();
        Debug.Log("[ZombieGameSetup] Tüm kurulum tamamlandı!");
        EditorUtility.DisplayDialog("ZombieGame Setup",
            "Kurulum tamamlandı!\n\nYapılacaklar:\n" +
            "• WaveManager → _wave1EnemyPrefab ata\n" +
            "• WaveManager → _wave2EnemyPrefab ata (opsiyonel)\n" +
            "• ZombieSpawner1'i deactivate et", "Tamam");
    }

    // ── Bölüm 1: UI ──────────────────────────────────────────────────────

    [MenuItem("Tools/ZombieGame/Setup UI (HUD + Panels)", false, 10)]
    static void SetupUI()
    {
        Canvas canvas = EnsureCanvas();

        CreateWaveHUD(canvas);
        CreateUpgradeSelectionPanel(canvas);
        CreateUpgradeSlotHUD(canvas);

        EditorUtility.SetDirty(canvas.gameObject);
        Debug.Log("[ZombieGameSetup] UI kurulumu tamamlandı.");
    }

    [MenuItem("Tools/ZombieGame/♻ Rebuild UI (Sil + Yeniden Oluştur)", false, 9)]
    static void RebuildUI()
    {
        Canvas canvas = EnsureCanvas();

        // Mevcut UI objeleri sil
        string[] toDelete = { "WaveHUD", "UpgradeSelectionPanel", "UpgradeSlotHUD" };
        foreach (string name in toDelete)
        {
            Transform existing = canvas.transform.Find(name);
            if (existing != null)
            {
                Undo.DestroyObjectImmediate(existing.gameObject);
                Debug.Log($"[ZombieGameSetup] {name} silindi.");
            }
        }

        // Yeniden oluştur
        CreateWaveHUD(canvas);
        CreateUpgradeSelectionPanel(canvas);
        CreateUpgradeSlotHUD(canvas);

        // UpgradeData'lar varsa yeniden bağla
        var allAssets = new System.Collections.Generic.List<UpgradeData>();
        string[] guids = AssetDatabase.FindAssets("t:UpgradeData");
        foreach (string guid in guids)
        {
            var a = AssetDatabase.LoadAssetAtPath<UpgradeData>(AssetDatabase.GUIDToAssetPath(guid));
            if (a != null) allAssets.Add(a);
        }
        if (allAssets.Count > 0) AutoWireUpgradeSelectionUI(allAssets);

        // UpgradeCard prefabı varsa bağla
        string[] cardGuids = AssetDatabase.FindAssets("UpgradeCard t:Prefab");
        if (cardGuids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(cardGuids[0]);
            var cardUI = AssetDatabase.LoadAssetAtPath<GameObject>(path)?.GetComponent<UpgradeCardUI>();
            var selUI  = Object.FindFirstObjectByType<UpgradeSelectionUI>(FindObjectsInactive.Include);
            if (cardUI != null && selUI != null)
            {
                var so = new SerializedObject(selUI);
                so.FindProperty("_cardPrefab").objectReferenceValue = cardUI;
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(selUI);
            }
        }

        WireWaveManagerToUI();
        EditorUtility.SetDirty(canvas.gameObject);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[ZombieGameSetup] UI yeniden oluşturuldu.");
    }

    // ── Bölüm 2: WaveManager ─────────────────────────────────────────────

    [MenuItem("Tools/ZombieGame/Setup WaveManager", false, 11)]
    static void SetupWaveManager()
    {
        // WaveManager zaten var mı?
        var existing = Object.FindFirstObjectByType<WaveManager>();
        if (existing != null)
        {
            Debug.Log("[ZombieGameSetup] WaveManager zaten var — atlandı.");
        }
        else
        {
            GameObject go = new GameObject("WaveManager");
            go.AddComponent<WaveManager>();
            go.AddComponent<EnemySpawner>();
            Undo.RegisterCreatedObjectUndo(go, "Create WaveManager");
            Debug.Log("[ZombieGameSetup] WaveManager oluşturuldu.");
        }

        // Var olan veya yeni oluşturulan ile UI referanslarını bağla
        WireWaveManagerToUI();

        // Eğer ZombieSpawner1 varsa uyar
        var spawner1 = Object.FindFirstObjectByType<ZombieSpawner1>();
        if (spawner1 != null && spawner1.enabled)
        {
            bool disable = EditorUtility.DisplayDialog("ZombieSpawner1 Bulundu",
                "Sahnede aktif ZombieSpawner1 var. WaveManager ile çakışır.\n\nDeactivate edilsin mi?",
                "Evet, Kapat", "Hayır");
            if (disable)
            {
                spawner1.enabled = false;
                EditorUtility.SetDirty(spawner1);
                Debug.Log("[ZombieGameSetup] ZombieSpawner1 deactivate edildi.");
            }
        }
    }

    // ── Bölüm 3: Player ──────────────────────────────────────────────────

    [MenuItem("Tools/ZombieGame/Setup Player Components", false, 12)]
    static void SetupPlayerComponents()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("[ZombieGameSetup] 'Player' tag'li GameObject bulunamadı. Tag'i kontrol et.");
            return;
        }

        bool changed = false;

        if (player.GetComponent<GunModifierStack>() == null)
        {
            Undo.AddComponent<GunModifierStack>(player);
            changed = true;
            Debug.Log("[ZombieGameSetup] GunModifierStack eklendi.");
        }

        if (player.GetComponent<UpgradeSlotManager>() == null)
        {
            Undo.AddComponent<UpgradeSlotManager>(player);
            changed = true;
            Debug.Log("[ZombieGameSetup] UpgradeSlotManager eklendi.");
        }

        if (player.GetComponent<EvolutionRegistry>() == null)
        {
            Undo.AddComponent<EvolutionRegistry>(player);
            changed = true;
            Debug.Log("[ZombieGameSetup] EvolutionRegistry eklendi.");
        }

        if (player.GetComponent<PlayerMovementModifiers>() == null)
        {
            Undo.AddComponent<PlayerMovementModifiers>(player);
            changed = true;
            Debug.Log("[ZombieGameSetup] PlayerMovementModifiers eklendi.");
        }

        if (!changed)
            Debug.Log("[ZombieGameSetup] Player component'leri zaten kurulu — değişiklik yok.");
        else
            EditorUtility.SetDirty(player);
    }

    // ═════════════════════════════════════════════════════════════════════
    // UI OLUŞTURUCULAR
    // ═════════════════════════════════════════════════════════════════════

    // ── WaveHUD ──────────────────────────────────────────────────────────

    static void CreateWaveHUD(Canvas canvas)
    {
        const string HUD_NAME = "WaveHUD";
        if (canvas.transform.Find(HUD_NAME) != null)
        {
            Debug.Log("[ZombieGameSetup] WaveHUD zaten var — atlandı.");
            return;
        }

        GameObject root = CreateUIObject(HUD_NAME, canvas.transform);
        StretchFull(root);
        WaveHUD hudScript = root.AddComponent<WaveHUD>();

        // ── Persistent Label (sol üst köşe) ──────────────────────────────
        GameObject persObj = CreateUIObject("PersistentLabel", root.transform);
        var persRT = persObj.GetComponent<RectTransform>();
        persRT.anchorMin  = new Vector2(0f, 1f);
        persRT.anchorMax  = new Vector2(0f, 1f);
        persRT.pivot      = new Vector2(0f, 1f);
        persRT.anchoredPosition = new Vector2(20f, -20f);
        persRT.sizeDelta  = new Vector2(200f, 40f);
        var persTMP = persObj.AddComponent<TextMeshProUGUI>();
        persTMP.text      = "Wave 1";
        persTMP.fontSize  = 22f;
        persTMP.fontStyle = FontStyles.Bold;
        persTMP.color     = new Color(1f, 0.9f, 0.2f);

        // ── Announcement Label (ekran ortası) ────────────────────────────
        GameObject annObj = CreateUIObject("AnnouncementLabel", root.transform);
        var annRT = annObj.GetComponent<RectTransform>();
        annRT.anchorMin  = new Vector2(0.5f, 0.5f);
        annRT.anchorMax  = new Vector2(0.5f, 0.5f);
        annRT.pivot      = new Vector2(0.5f, 0.5f);
        annRT.anchoredPosition = Vector2.zero;
        annRT.sizeDelta  = new Vector2(600f, 200f);
        var annTMP = annObj.AddComponent<TextMeshProUGUI>();
        annTMP.text      = "WAVE 1";
        annTMP.fontSize  = 72f;
        annTMP.fontStyle = FontStyles.Bold;
        annTMP.alignment = TextAlignmentOptions.Center;
        annTMP.color     = Color.white;
        var annColor = annTMP.color;
        annColor.a       = 0f;
        annTMP.color     = annColor;

        // Referansları bağla (SerializedObject ile)
        SerializedObject so = new SerializedObject(hudScript);
        so.FindProperty("_persistentLabel").objectReferenceValue  = persTMP;
        so.FindProperty("_announcementLabel").objectReferenceValue = annTMP;
        so.ApplyModifiedProperties();

        Undo.RegisterCreatedObjectUndo(root, "Create WaveHUD");
        Debug.Log("[ZombieGameSetup] WaveHUD oluşturuldu.");
    }

    // ── UpgradeSelectionPanel ─────────────────────────────────────────────

    static void CreateUpgradeSelectionPanel(Canvas canvas)
    {
        const string PANEL_NAME = "UpgradeSelectionPanel";
        if (canvas.transform.Find(PANEL_NAME) != null)
        {
            Debug.Log("[ZombieGameSetup] UpgradeSelectionPanel zaten var — atlandı.");
            return;
        }

        // ── Panel (tam ekran karartma) ────────────────────────────────────
        GameObject panel = CreateUIObject(PANEL_NAME, canvas.transform);
        StretchFull(panel);
        var panelImg = panel.AddComponent<Image>();
        panelImg.color = new Color(0.04f, 0.04f, 0.08f, 0.92f);
        var cg = panel.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        panel.SetActive(false);

        // ── Başlık — üst %15'lik şerit ───────────────────────────────────
        GameObject titleObj = CreateUIObject("TitleLabel", panel.transform);
        var titleRT = titleObj.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0.1f, 0.82f);
        titleRT.anchorMax = new Vector2(0.9f, 0.97f);
        titleRT.offsetMin = Vector2.zero;
        titleRT.offsetMax = Vector2.zero;
        var titleTMP = titleObj.AddComponent<TextMeshProUGUI>();
        titleTMP.text      = "WAVE X TAMAMLANDI — Bir Upgrade Seç";
        titleTMP.fontSize  = 40f;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.color     = new Color(1f, 0.92f, 0.3f);
        titleTMP.enableAutoSizing = true;
        titleTMP.fontSizeMin = 20f;
        titleTMP.fontSizeMax = 52f;

        // ── Kart Container — ekranın %85 genişliği, orta %65 yüksekliği ──
        GameObject container = CreateUIObject("CardContainer", panel.transform);
        var contRT = container.GetComponent<RectTransform>();
        contRT.anchorMin = new Vector2(0.075f, 0.13f);
        contRT.anchorMax = new Vector2(0.925f, 0.80f);
        contRT.offsetMin = Vector2.zero;
        contRT.offsetMax = Vector2.zero;

        // GridLayoutGroup — 4 sütun, otomatik hücre boyutu
        var glg = container.AddComponent<GridLayoutGroup>();
        glg.constraint           = GridLayoutGroup.Constraint.FixedColumnCount;
        glg.constraintCount      = 4;
        glg.spacing              = new Vector2(18f, 18f);
        glg.padding              = new RectOffset(10, 10, 10, 10);
        glg.childAlignment       = TextAnchor.MiddleCenter;
        // cellSize: kartlar GridLayoutGroup tarafından boyutlandırılır,
        // ContentSizeFitter ile dinamik hücre için (200×280 referans)
        glg.cellSize             = new Vector2(200f, 280f);
        EditorUtility.SetDirty(container);

        // İpucu metni — alt %10
        GameObject hintObj = CreateUIObject("HintLabel", panel.transform);
        var hintRT = hintObj.GetComponent<RectTransform>();
        hintRT.anchorMin = new Vector2(0.1f, 0.03f);
        hintRT.anchorMax = new Vector2(0.9f, 0.12f);
        hintRT.offsetMin = Vector2.zero;
        hintRT.offsetMax = Vector2.zero;
        var hintTMP = hintObj.AddComponent<TextMeshProUGUI>();
        hintTMP.text      = "Bir karta tıkla veya [1-4] ile seç";
        hintTMP.fontSize  = 18f;
        hintTMP.alignment = TextAlignmentOptions.Center;
        hintTMP.color     = new Color(0.7f, 0.7f, 0.7f);

        // ── UpgradeSelectionUI scripti ────────────────────────────────────
        UpgradeSelectionUI uiScript = panel.AddComponent<UpgradeSelectionUI>();
        SerializedObject so = new SerializedObject(uiScript);
        so.FindProperty("_panel").objectReferenceValue         = panel;
        so.FindProperty("_canvasGroup").objectReferenceValue   = cg;
        so.FindProperty("_titleLabel").objectReferenceValue    = titleTMP;
        so.FindProperty("_cardContainer").objectReferenceValue = container.transform;
        so.ApplyModifiedProperties();

        // UpgradeCard prefabını bul ve ata (varsa)
        string[] cardGuids = AssetDatabase.FindAssets("UpgradeCard t:Prefab");
        if (cardGuids.Length > 0)
        {
            string path    = AssetDatabase.GUIDToAssetPath(cardGuids[0]);
            var cardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var cardUI     = cardPrefab?.GetComponent<UpgradeCardUI>();
            if (cardUI != null)
            {
                so = new SerializedObject(uiScript);
                so.FindProperty("_cardPrefab").objectReferenceValue = cardUI;
                so.ApplyModifiedProperties();
                Debug.Log("[ZombieGameSetup] UpgradeCard prefabı otomatik bağlandı.");
            }
        }
        else
            Debug.Log("[ZombieGameSetup] UpgradeCard prefabı bulunamadı — CreateUpgradeCardPrefab'ı çalıştır.");

        Undo.RegisterCreatedObjectUndo(panel, "Create UpgradeSelectionPanel");
        Debug.Log("[ZombieGameSetup] UpgradeSelectionPanel oluşturuldu.");
    }

    // ── UpgradeSlotHUD ────────────────────────────────────────────────────

    static void CreateUpgradeSlotHUD(Canvas canvas)
    {
        const string HUD_NAME = "UpgradeSlotHUD";
        if (canvas.transform.Find(HUD_NAME) != null)
        {
            Debug.Log("[ZombieGameSetup] UpgradeSlotHUD zaten var — atlandı.");
            return;
        }

        // Root: ekranın alt %12'si, yatayda %25-75 arası (merkez %50 genişlik)
        GameObject root = CreateUIObject(HUD_NAME, canvas.transform);
        var rootRT = root.GetComponent<RectTransform>();
        rootRT.anchorMin = new Vector2(0.25f, 0f);
        rootRT.anchorMax = new Vector2(0.75f, 0f);
        rootRT.pivot     = new Vector2(0.5f, 0f);
        rootRT.offsetMin = new Vector2(0f, 8f);
        rootRT.offsetMax = new Vector2(0f, 8f);
        rootRT.sizeDelta = new Vector2(0f, 95f);  // yükseklik sabit, genişlik anchor ile

        // HLG: çocukları eşit böl, childControlWidth=true + childForceExpandWidth=true
        var hlg = root.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing                = 8f;
        hlg.padding                = new RectOffset(8, 8, 4, 4);
        hlg.childAlignment         = TextAnchor.MiddleCenter;
        hlg.childControlWidth      = true;
        hlg.childControlHeight     = true;
        hlg.childForceExpandWidth  = true;
        hlg.childForceExpandHeight = true;
        EditorUtility.SetDirty(root);

        UpgradeSlotHUD hudScript = root.AddComponent<UpgradeSlotHUD>();
        SerializedObject so = new SerializedObject(hudScript);
        var slotsProp = so.FindProperty("_slots");
        slotsProp.arraySize = 4;

        Color[] catColors = {
            new Color(0.42f, 0.71f, 1f, 0.18f),
            new Color(0.42f, 0.71f, 1f, 0.18f),
            new Color(0.42f, 0.71f, 1f, 0.18f),
            new Color(0.42f, 0.71f, 1f, 0.18f),
        };

        for (int i = 0; i < 4; i++)
        {
            GameObject slot = CreateUIObject($"Slot{i}", root.transform);
            // LayoutElement: minimum boyut garantisi
            var le = slot.AddComponent<LayoutElement>();
            le.minWidth  = 80f;
            le.minHeight = 70f;
            le.flexibleWidth = 1f;  // eşit büyü

            var slotImg = slot.AddComponent<Image>();
            slotImg.color = new Color(0.08f, 0.08f, 0.12f, 0.88f);

            // Üst renk çubuğu
            GameObject barObj = CreateUIObject("CategoryBar", slot.transform);
            var barRT = barObj.GetComponent<RectTransform>();
            barRT.anchorMin = new Vector2(0f, 0.88f);
            barRT.anchorMax = new Vector2(1f, 1f);
            barRT.offsetMin = Vector2.zero;
            barRT.offsetMax = Vector2.zero;
            var barImg = barObj.AddComponent<Image>();
            barImg.color = catColors[i];

            // Empty overlay
            GameObject emptyObj = CreateUIObject("EmptyOverlay", slot.transform);
            StretchFull(emptyObj);
            var emptyImg = emptyObj.AddComponent<Image>();
            emptyImg.color = new Color(1f, 1f, 1f, 0.03f);

            // İkon — üst %55
            GameObject iconObj = CreateUIObject("Icon", slot.transform);
            var iconRT = iconObj.GetComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0.15f, 0.42f);
            iconRT.anchorMax = new Vector2(0.85f, 0.86f);
            iconRT.offsetMin = Vector2.zero;
            iconRT.offsetMax = Vector2.zero;
            var iconImg = iconObj.AddComponent<Image>();
            iconImg.color = new Color(0.3f, 0.3f, 0.35f, 0.6f);

            // Slot no — sol üst
            GameObject numObj = CreateUIObject("Number", slot.transform);
            var numRT = numObj.GetComponent<RectTransform>();
            numRT.anchorMin = new Vector2(0f, 0.88f);
            numRT.anchorMax = new Vector2(0.45f, 1f);
            numRT.offsetMin = new Vector2(3f, -1f);
            numRT.offsetMax = new Vector2(-1f, -1f);
            var numTMP = numObj.AddComponent<TextMeshProUGUI>();
            numTMP.text      = $"{i + 1}";
            numTMP.fontSize  = 11f;
            numTMP.fontStyle = FontStyles.Bold;
            numTMP.alignment = TextAlignmentOptions.Left;
            numTMP.color     = new Color(1f, 1f, 1f, 0.4f);

            // İsim — alt %42
            GameObject nameObj = CreateUIObject("NameLabel", slot.transform);
            var nameRT = nameObj.GetComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0f, 0f);
            nameRT.anchorMax = new Vector2(1f, 0.42f);
            nameRT.offsetMin = new Vector2(3f, 2f);
            nameRT.offsetMax = new Vector2(-3f, 0f);
            var nameTMP = nameObj.AddComponent<TextMeshProUGUI>();
            nameTMP.text             = "SLOT";
            nameTMP.fontSize         = 10f;
            nameTMP.enableAutoSizing = true;
            nameTMP.fontSizeMin      = 7f;
            nameTMP.fontSizeMax      = 13f;
            nameTMP.alignment        = TextAlignmentOptions.Center;
            nameTMP.color            = new Color(0.65f, 0.65f, 0.65f);

            // SlotEntry
            var slotEntry = slotsProp.GetArrayElementAtIndex(i);
            slotEntry.FindPropertyRelative("root").objectReferenceValue         = slot;
            slotEntry.FindPropertyRelative("iconImage").objectReferenceValue    = iconImg;
            slotEntry.FindPropertyRelative("nameLabel").objectReferenceValue    = nameTMP;
            slotEntry.FindPropertyRelative("emptyOverlay").objectReferenceValue = emptyObj;
        }

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(hudScript);

        Undo.RegisterCreatedObjectUndo(root, "Create UpgradeSlotHUD");
        Debug.Log("[ZombieGameSetup] UpgradeSlotHUD oluşturuldu.");
    }

    // ── UpgradeData Assetleri ─────────────────────────────────────────────

    [MenuItem("Tools/ZombieGame/🧹 Remove Missing Scripts (Scene)", false, 40)]
    static void CleanMissingScripts()
    {
        int totalRemoved = 0;
        var allObjects = Object.FindObjectsByType<GameObject>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var go in allObjects)
        {
            int removed = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
            if (removed > 0)
            {
                Debug.Log($"[ZombieGameSetup] '{go.name}' → {removed} missing script(s) silindi.");
                EditorUtility.SetDirty(go);
                totalRemoved += removed;
            }
        }

        if (totalRemoved == 0)
            Debug.Log("[ZombieGameSetup] Missing script bulunamadı.");
        else
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            Debug.Log($"[ZombieGameSetup] Toplam {totalRemoved} missing script silindi.");
        }
    }

    [MenuItem("Tools/ZombieGame/Create All UpgradeData Assets", false, 29)]
    static void CreateAllUpgradeAssets()
    {
        const string basePath = "Assets/Erdem/2nd/ScriptableObjects/Upgrades";
        System.IO.Directory.CreateDirectory(basePath + "/Movement");
        System.IO.Directory.CreateDirectory(basePath + "/Gun");
        System.IO.Directory.CreateDirectory(basePath + "/Utility");
        System.IO.Directory.CreateDirectory(basePath + "/Evolution");
        AssetDatabase.Refresh();

        // (name, category, behaviourType, description, folder)
        var defs = new (string name, UpgradeCategory cat, string type, string desc, string folder)[]
        {
            // Movement
            ("Jetpack",        UpgradeCategory.Movement, "JetpackUpgrade",        "Space tuşuna basılı tutunca uçarsın.",           "Movement"),
            ("Double Dash",    UpgradeCategory.Movement, "DoubleDashUpgrade",     "E tuşu ile iki kez art arda dash at.",           "Movement"),
            ("Slide",          UpgradeCategory.Movement, "SlideUpgrade",          "C tuşu ile kayar, düşmanları iter.",             "Movement"),
            ("Wall Run",       UpgradeCategory.Movement, "WallRunUpgrade",        "Duvarlarda koşabilirsin.",                       "Movement"),
            ("Ground Slam",    UpgradeCategory.Movement, "GroundSlamUpgrade",     "Havada C ile yere çarparak alan hasarı ver.",    "Movement"),
            ("Grapple",        UpgradeCategory.Movement, "GrappleUpgrade",        "Q ile kendinizi hedefe fırlat.",                 "Movement"),
            ("Phase Step",     UpgradeCategory.Movement, "PhaseStepUpgrade",      "F ile kısa mesafeye anında ışınlan (2 şarj).",  "Movement"),
            ("Momentum Surge", UpgradeCategory.Movement, "MomentumSurgeUpgrade",  "Sprint hızın ve süresi artar.",                  "Movement"),
            // Gun
            ("Spread Shot",    UpgradeCategory.Gun,      "SpreadShotUpgrade",     "Her atışta koni şeklinde 2 ek mermi çıkar.",    "Gun"),
            ("Rapid Fire",     UpgradeCategory.Gun,      "RapidFireUpgrade",      "Ateş hızı +%50, hasar -%15.",                   "Gun"),
            ("High Caliber",   UpgradeCategory.Gun,      "HighCaliberUpgrade",    "Hasar +%80, ateş hızı -%40.",                   "Gun"),
            ("Piercer",        UpgradeCategory.Gun,      "PiercerUpgrade",        "Mermi 3 düşmandan geçer.",                      "Gun"),
            ("Ricochet",       UpgradeCategory.Gun,      "RicochetUpgrade",       "Mermi duvarlara çarpıp 2 kez yansır.",          "Gun"),
            // Utility
            ("Armor",          UpgradeCategory.Utility,  "ArmorUpgrade",          "Gelen tüm hasar -%15.",                         "Utility"),
            // Evolution
            ("Shoulder Bash",  UpgradeCategory.Evolution,"ShoulderBashEvolution", "[Jetpack + Double Dash] Çarpışmada alan hasarı.","Evolution"),
            ("Predator Drop",  UpgradeCategory.Evolution,"PredatorDropEvolution", "[Grapple + Ground Slam] Düşen bomba etkisi.",   "Evolution"),
            ("Cannonball",     UpgradeCategory.Evolution,"CannonballEvolution",   "[Slide + Armor] Kayarken çevreye hasar ver.",   "Evolution"),
            ("Ghost Double",   UpgradeCategory.Evolution,"GhostDoubleEvolution",  "[Phase Step + Armor] Işınlanınca tuzak bırak.", "Evolution"),
        };

        int created = 0;
        int skipped = 0;
        var allAssets = new System.Collections.Generic.List<UpgradeData>();

        foreach (var d in defs)
        {
            string safeName = d.name.Replace(" ", "");
            string path     = $"{basePath}/{d.folder}/{safeName}_UpgradeData.asset";

            var existing = AssetDatabase.LoadAssetAtPath<UpgradeData>(path);
            if (existing != null) { allAssets.Add(existing); skipped++; continue; }

            var asset = ScriptableObject.CreateInstance<UpgradeData>();
            SerializedObject so = new SerializedObject(asset);
            so.FindProperty("_upgradeName").stringValue      = d.name;
            so.FindProperty("_description").stringValue      = d.desc;
            so.FindProperty("_category").enumValueIndex      = (int)d.cat;
            so.FindProperty("_behaviourTypeName").stringValue = d.type;
            so.ApplyModifiedProperties();

            AssetDatabase.CreateAsset(asset, path);
            allAssets.Add(asset);
            created++;
        }

        // Evolution prerequisitlerini bağla (ShoulderBash: Jetpack+DoubleDash vb.)
        SetEvolutionPrereqs(basePath, allAssets);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // UpgradeSelectionUI'ya tüm asset'leri bağla
        AutoWireUpgradeSelectionUI(allAssets);

        Debug.Log($"[ZombieGameSetup] UpgradeData: {created} oluşturuldu, {skipped} atlandı.");
        EditorUtility.DisplayDialog("Upgrade Assets",
            $"{created} yeni asset oluşturuldu, {skipped} zaten vardı.\n\nUpgradeSelectionUI._allUpgrades otomatik dolduruldu.",
            "Tamam");
    }

    static void SetEvolutionPrereqs(string basePath, System.Collections.Generic.List<UpgradeData> assets)
    {
        UpgradeData Find(string name) => assets.Find(a => a != null && a.UpgradeName == name);

        var evoPairs = new (string evo, string[] reqs)[]
        {
            ("Shoulder Bash", new[] { "Jetpack", "Double Dash" }),
            ("Predator Drop", new[] { "Grapple", "Ground Slam" }),
            ("Cannonball",    new[] { "Slide", "Armor" }),
            ("Ghost Double",  new[] { "Phase Step", "Armor" }),
        };

        foreach (var (evoName, reqNames) in evoPairs)
        {
            var evo = Find(evoName);
            if (evo == null) continue;

            var reqs = new System.Collections.Generic.List<UpgradeData>();
            foreach (var rn in reqNames)
            {
                var req = Find(rn);
                if (req != null) reqs.Add(req);
            }

            SerializedObject so = new SerializedObject(evo);
            var prereqProp = so.FindProperty("_prerequisites");
            prereqProp.arraySize = reqs.Count;
            for (int i = 0; i < reqs.Count; i++)
                prereqProp.GetArrayElementAtIndex(i).objectReferenceValue = reqs[i];
            so.ApplyModifiedProperties();
        }
    }

    static void AutoWireUpgradeSelectionUI(System.Collections.Generic.List<UpgradeData> assets)
    {
        var selUI = Object.FindFirstObjectByType<UpgradeSelectionUI>(FindObjectsInactive.Include);
        if (selUI == null) { Debug.Log("[ZombieGameSetup] UpgradeSelectionUI sahnede yok — önce Setup UI çalıştır."); return; }

        SerializedObject so = new SerializedObject(selUI);
        var prop = so.FindProperty("_allUpgrades");
        prop.arraySize = assets.Count;
        for (int i = 0; i < assets.Count; i++)
            prop.GetArrayElementAtIndex(i).objectReferenceValue = assets[i];
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(selUI);
        Debug.Log("[ZombieGameSetup] UpgradeSelectionUI._allUpgrades dolduruldu.");
    }

    // ── UpgradeCard Prefabı ───────────────────────────────────────────────

    [MenuItem("Tools/ZombieGame/Create UpgradeCard Prefab", false, 30)]
    static void CreateUpgradeCardPrefab()
    {
        string prefabPath = "Assets/Erdem/2nd/Prefabs/UI/UpgradeCard.prefab";

        // Klasörü oluştur
        System.IO.Directory.CreateDirectory("Assets/Erdem/2nd/Prefabs/UI");
        AssetDatabase.Refresh();

        // Daha önce oluşturulmuş mu? Varsa sadece wire et ve çık.
        var existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (existingPrefab != null)
        {
            var existingCardUI = existingPrefab.GetComponent<UpgradeCardUI>();
            if (existingCardUI != null)
            {
                var selUIExisting = Object.FindFirstObjectByType<UpgradeSelectionUI>(FindObjectsInactive.Include);
                if (selUIExisting != null)
                {
                    var soExisting = new SerializedObject(selUIExisting);
                    soExisting.FindProperty("_cardPrefab").objectReferenceValue = existingCardUI;
                    soExisting.ApplyModifiedProperties();
                    EditorUtility.SetDirty(selUIExisting);
                    Debug.Log("[ZombieGameSetup] UpgradeCard prefabı zaten var — UpgradeSelectionUI'ya bağlandı.");
                }
            }
            return;
        }

        // Geçici sahne objesi oluştur
        Canvas canvas = EnsureCanvas();

        GameObject card = CreateUIObject("UpgradeCard", canvas.transform);
        var cardRT = card.GetComponent<RectTransform>();
        cardRT.sizeDelta = new Vector2(200f, 280f);
        var cardImg = card.AddComponent<Image>();
        cardImg.color = new Color(0.12f, 0.12f, 0.15f, 1f);
        var btn = card.AddComponent<Button>();
        var colors = btn.colors;
        colors.normalColor      = new Color(0.12f, 0.12f, 0.15f);
        colors.highlightedColor = new Color(0.2f, 0.2f, 0.28f);
        colors.pressedColor     = new Color(0.3f, 0.3f, 0.4f);
        btn.colors = colors;

        // Kategori renk çubuğu (sol kenar)
        GameObject colorBar = CreateUIObject("CategoryColorBar", card.transform);
        var barRT = colorBar.GetComponent<RectTransform>();
        barRT.anchorMin = new Vector2(0f, 0f);
        barRT.anchorMax = new Vector2(0f, 1f);
        barRT.pivot     = new Vector2(0f, 0.5f);
        barRT.offsetMin = new Vector2(0f, 0f);
        barRT.offsetMax = new Vector2(6f, 0f);
        var barImg = colorBar.AddComponent<Image>();
        barImg.color = new Color(1f, 0.42f, 0.42f);

        // İkon alanı
        GameObject iconObj = CreateUIObject("IconImage", card.transform);
        var iconRT = iconObj.GetComponent<RectTransform>();
        iconRT.anchorMin = new Vector2(0.5f, 0.62f);
        iconRT.anchorMax = new Vector2(0.5f, 0.95f);
        iconRT.pivot     = new Vector2(0.5f, 0.5f);
        iconRT.offsetMin = new Vector2(-50f, 0f);
        iconRT.offsetMax = new Vector2(50f,  0f);
        var iconImg = iconObj.AddComponent<Image>();
        iconImg.color = new Color(0.4f, 0.4f, 0.5f);

        // İsim
        GameObject nameObj = CreateUIObject("NameLabel", card.transform);
        var nameRT = nameObj.GetComponent<RectTransform>();
        nameRT.anchorMin = new Vector2(0f, 0.42f);
        nameRT.anchorMax = new Vector2(1f, 0.62f);
        nameRT.pivot     = new Vector2(0.5f, 0.5f);
        nameRT.offsetMin = new Vector2(10f, 0f);
        nameRT.offsetMax = new Vector2(-10f, 0f);
        var nameTMP = nameObj.AddComponent<TextMeshProUGUI>();
        nameTMP.text      = "Upgrade Adı";
        nameTMP.fontSize  = 16f;
        nameTMP.fontStyle = FontStyles.Bold;
        nameTMP.alignment = TextAlignmentOptions.Center;
        nameTMP.color     = Color.white;

        // Açıklama
        GameObject descObj = CreateUIObject("DescLabel", card.transform);
        var descRT = descObj.GetComponent<RectTransform>();
        descRT.anchorMin = new Vector2(0f, 0.08f);
        descRT.anchorMax = new Vector2(1f, 0.42f);
        descRT.pivot     = new Vector2(0.5f, 0.5f);
        descRT.offsetMin = new Vector2(12f, 0f);
        descRT.offsetMax = new Vector2(-12f, 0f);
        var descTMP = descObj.AddComponent<TextMeshProUGUI>();
        descTMP.text      = "Upgrade açıklaması";
        descTMP.fontSize  = 12f;
        descTMP.alignment = TextAlignmentOptions.Center;
        descTMP.color     = new Color(0.8f, 0.8f, 0.8f);
        descTMP.enableWordWrapping = true;

        // UpgradeCardUI
        UpgradeCardUI cardUI = card.AddComponent<UpgradeCardUI>();
        SerializedObject so = new SerializedObject(cardUI);
        so.FindProperty("_iconImage").objectReferenceValue        = iconImg;
        so.FindProperty("_nameLabel").objectReferenceValue        = nameTMP;
        so.FindProperty("_descLabel").objectReferenceValue        = descTMP;
        so.FindProperty("_categoryColorBar").objectReferenceValue = barImg;
        so.FindProperty("_button").objectReferenceValue           = btn;
        so.ApplyModifiedProperties();

        // Prefab kaydet
        bool success;
        PrefabUtility.SaveAsPrefabAsset(card, prefabPath, out success);
        Object.DestroyImmediate(card);

        if (success)
        {
            Debug.Log($"[ZombieGameSetup] UpgradeCard prefabı oluşturuldu: {prefabPath}");

            // UpgradeSelectionUI'ya otomatik bağla
            var selUI = Object.FindFirstObjectByType<UpgradeSelectionUI>(FindObjectsInactive.Include);
            if (selUI != null)
            {
                var cardPrefabObj = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                var cardUIPrefab  = cardPrefabObj.GetComponent<UpgradeCardUI>();
                SerializedObject selSO = new SerializedObject(selUI);
                selSO.FindProperty("_cardPrefab").objectReferenceValue = cardUIPrefab;
                selSO.ApplyModifiedProperties();
                EditorUtility.SetDirty(selUI);
                Debug.Log("[ZombieGameSetup] UpgradeCard prefabı UpgradeSelectionUI'ya bağlandı.");
            }
        }
        else
            Debug.LogError("[ZombieGameSetup] UpgradeCard prefab kaydedilemedi.");
    }

    // ═════════════════════════════════════════════════════════════════════
    // YARDIMCI METODLAR
    // ═════════════════════════════════════════════════════════════════════

    static Canvas EnsureCanvas()
    {
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas != null)
        {
            // Mevcut CanvasScaler'ı Scale With Screen Size'a güncelle
            var existingScaler = canvas.GetComponent<CanvasScaler>();
            if (existingScaler != null) ConfigureScaler(existingScaler);
            return canvas;
        }

        GameObject go = new GameObject("Canvas");
        canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        ConfigureScaler(go.AddComponent<CanvasScaler>());
        go.AddComponent<GraphicRaycaster>();
        Undo.RegisterCreatedObjectUndo(go, "Create Canvas");
        return canvas;
    }

    static void ConfigureScaler(CanvasScaler scaler)
    {
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution  = new Vector2(1920f, 1080f);
        scaler.screenMatchMode      = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight   = 0.5f;  // genişlik+yükseklik dengeli ölçekleme
        EditorUtility.SetDirty(scaler);
    }

    static GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    static void StretchFull(GameObject go)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    static void WireWaveManagerToUI()
    {
        WaveManager wm = Object.FindFirstObjectByType<WaveManager>(FindObjectsInactive.Include);
        if (wm == null) return;

        SerializedObject so = new SerializedObject(wm);

        // EnemySpawner — genellikle aynı GameObject'te
        var es = wm.GetComponent<EnemySpawner>();
        if (es != null)
            so.FindProperty("_spawner").objectReferenceValue = es;

        // WaveHUD
        WaveHUD hud = Object.FindFirstObjectByType<WaveHUD>(FindObjectsInactive.Include);
        if (hud != null)
            so.FindProperty("_waveHUD").objectReferenceValue = hud;

        // UpgradeSelectionUI
        UpgradeSelectionUI selUI = Object.FindFirstObjectByType<UpgradeSelectionUI>(FindObjectsInactive.Include);
        if (selUI != null)
            so.FindProperty("_upgradeSelectionUI").objectReferenceValue = selUI;

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(wm);
    }
}
#endif
