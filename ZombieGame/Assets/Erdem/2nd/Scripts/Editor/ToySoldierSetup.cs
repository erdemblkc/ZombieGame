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
