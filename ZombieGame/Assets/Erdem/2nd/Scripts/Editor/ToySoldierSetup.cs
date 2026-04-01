using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

/// <summary>
/// Tools → ZombieGame → Create ToySoldier Animator Controller
/// Prefabs/ToySoldier/ klasöründeki FBX animasyonlarını otomatik bağlar.
/// </summary>
public static class ToySoldierSetup
{
    private const string FolderPath  = "Assets/Erdem/2nd/Prefabs/ToySoldier/";
    private const string SavePath    = "Assets/Erdem/2nd/Prefabs/ToySoldier/ToySoldierAnimator.controller";

    [MenuItem("Tools/ZombieGame/Create ToySoldier Wave Configs")]
    public static void CreateWaveConfigs()
    {
        string folder = "Assets/Erdem/2nd/ScriptableObjects/";

        // Wave 1 — Living Room: 5 asker
        var w1 = ScriptableObject.CreateInstance<WaveConfig>();
        w1.waveNumber        = 1;
        w1.timeBetweenSpawns = 0.8f;
        w1.speedMultiplier   = 1f;
        w1.damageMultiplier  = 1f;
        w1.healthMultiplier  = 1f;
        // enemies listesi Inspector'dan doldurulacak
        AssetDatabase.CreateAsset(w1, folder + "WaveConfig_LivingRoom.asset");

        // Wave 2 — Bedroom: 8 asker
        var w2 = ScriptableObject.CreateInstance<WaveConfig>();
        w2.waveNumber        = 2;
        w2.timeBetweenSpawns = 0.65f;
        w2.speedMultiplier   = 1.2f;
        w2.damageMultiplier  = 1.1f;
        w2.healthMultiplier  = 1.2f;
        AssetDatabase.CreateAsset(w2, folder + "WaveConfig_Bedroom.asset");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[ToySoldierSetup] WaveConfig'ler oluşturuldu: " + folder);
        EditorUtility.DisplayDialog("ToySoldier Setup",
            "WaveConfig asset'leri oluşturuldu!\n\n" +
            "Her WaveConfig'te 'Enemies' listesine:\n" +
            "  Enemy Prefab: ToySoldierEnemy\n" +
            "  Count: 5 (LivingRoom) / 8 (Bedroom)\n\n" +
            "WaveManager → _waveConfigs dizisine ata.",
            "Tamam");
    }

    [MenuItem("Tools/ZombieGame/Create ToySoldier Animator Controller")]
    public static void CreateAnimatorController()
    {
        // ── 1. Clip'leri yükle ────────────────────────────────────────────
        AnimationClip runClip   = LoadClip(FolderPath + "Rifle Run.fbx");
        AnimationClip fireClip  = LoadClip(FolderPath + "Firing Rifle (1).fbx");
        AnimationClip deathClip = LoadClip(FolderPath + "Rifle Death.fbx");

        // ── 2. Controller oluştur ─────────────────────────────────────────
        var controller = AnimatorController.CreateAnimatorControllerAtPath(SavePath);

        // ── 3. Parametreler ───────────────────────────────────────────────
        controller.AddParameter("IsFiring", AnimatorControllerParameterType.Bool);
        controller.AddParameter("Die",      AnimatorControllerParameterType.Trigger);

        // ── 4. State'ler ──────────────────────────────────────────────────
        var sm = controller.layers[0].stateMachine;

        var runState   = sm.AddState("RifleRun");
        var fireState  = sm.AddState("FiringRifle");
        var deathState = sm.AddState("RifleDeath");

        if (runClip   != null) runState.motion   = runClip;
        if (fireClip  != null) fireState.motion  = fireClip;
        if (deathClip != null) deathState.motion = deathClip;

        sm.defaultState = runState;

        // ── 5. Transition'lar ─────────────────────────────────────────────

        // RifleRun → FiringRifle  (IsFiring = true)
        var toFire = runState.AddTransition(fireState);
        toFire.AddCondition(AnimatorConditionMode.If, 0f, "IsFiring");
        toFire.hasExitTime = false;
        toFire.duration    = 0.1f;

        // FiringRifle → RifleRun  (IsFiring = false)
        var toRun = fireState.AddTransition(runState);
        toRun.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsFiring");
        toRun.hasExitTime = false;
        toRun.duration    = 0.1f;

        // Any State → RifleDeath  (Die trigger) — canExit: true
        var toDeath = sm.AddAnyStateTransition(deathState);
        toDeath.AddCondition(AnimatorConditionMode.If, 0f, "Die");
        toDeath.hasExitTime   = false;
        toDeath.duration      = 0.1f;
        toDeath.canTransitionToSelf = false;

        // ── 6. Kaydet ─────────────────────────────────────────────────────
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[ToySoldierSetup] Animator Controller oluşturuldu: " + SavePath);
        EditorUtility.DisplayDialog(
            "ToySoldier Setup",
            "Animator Controller oluşturuldu!\n\nKonum:\n" + SavePath +
            "\n\nEksik clip varsa Console'da uyarı göreceksin.",
            "Tamam");

        Selection.activeObject = AssetDatabase.LoadAssetAtPath<AnimatorController>(SavePath);
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    static AnimationClip LoadClip(string path)
    {
        Object[] all = AssetDatabase.LoadAllAssetsAtPath(path);
        foreach (var obj in all)
        {
            if (obj is AnimationClip clip && !clip.name.StartsWith("__preview__"))
                return clip;
        }
        Debug.LogWarning("[ToySoldierSetup] Clip bulunamadı: " + path);
        return null;
    }
}
