using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Oyuncu koşarken global shader property'si üzerinden
/// RadialBlurFeature'ı kontrol eder.
/// </summary>
public class SprintBlurEffect : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Boş bırakırsan sahnedeki ilk PlayerController2'yi bulur")]
    public PlayerController2 player;

    [Header("Radial Blur")]
    [Tooltip("Koşarken blur gücü. Önerilen: 0.06 – 0.14")]
    [Range(0f, 0.2f)]
    public float maxStrength = 0.05f;

    [Header("Geçiş Hızları")]
    public float fadeInSpeed  = 10f;
    public float fadeOutSpeed = 6f;

    // ── Internals ─────────────────────────────────────────────────────────────
    private static readonly int StrengthId = Shader.PropertyToID("_RadialBlurStrength");
    private float _current;

    // ═════════════════════════════════════════════════════════════════════════

    private void Awake()
    {
        if (player == null)
            player = FindObjectOfType<PlayerController2>();
    }

    private void Update()
    {
        if (player == null) return;

        float target = player.IsSprinting ? maxStrength : 0f;
        float speed  = player.IsSprinting ? fadeInSpeed : fadeOutSpeed;

        _current = Mathf.Lerp(_current, target, Time.deltaTime * speed);
        Shader.SetGlobalFloat(StrengthId, _current);
    }

    private void OnDisable()
    {
        // Sahne kapanırken sıfırla
        Shader.SetGlobalFloat(StrengthId, 0f);
    }
}
