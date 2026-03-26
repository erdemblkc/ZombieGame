using System.Collections;
using UnityEngine;

/// <summary>
/// Plastik kırılma hit feedback — Toy Terror teması.
/// Vuruşta particle + ses çalar. Particle sistem yoksa renk flash fallback çalışır.
/// ZombieHealth1.TakeDamage içinden çağrılır veya standalone kullanılır.
/// </summary>
public class PlasticHitEffect : MonoBehaviour
{
    [Header("Particle (Erdem atayacak)")]
    [Tooltip("Plastic kırılma particle prefab — Erdem oluşturup buraya atayacak")]
    public ParticleSystem hitParticlePrefab;
    [Tooltip("Particle spawn offset (genelde merkez)")]
    public Vector3 spawnOffset = new Vector3(0f, 0.8f, 0f);

    [Header("Ses (Erdem atayacak)")]
    [Tooltip("Plastik kırılma sesleri — birinden rastgele çalar")]
    public AudioClip[] hitSounds;
    [Range(0f, 1f)]
    public float hitVolume = 0.7f;
    [Tooltip("Pitch varyasyonu — daha organik his")]
    public float pitchVariance = 0.15f;

    [Header("Fallback — Particle Yokken")]
    [Tooltip("Particle atanmamışsa renk flash çalışır")]
    public Color flashColor = Color.white;
    public float flashDuration = 0.08f;

    // ── Internal ──────────────────────────────────────────────────────────

    Renderer[]  renderers;
    AudioSource audioSource;
    Color[]     originalColors;
    bool        flashing;

    void Awake()
    {
        renderers   = GetComponentsInChildren<Renderer>();
        audioSource = GetComponent<AudioSource>();

        // Orijinal renkleri kaydet (flash sonrası geri dön)
        CacheOriginalColors();
    }

    // ── Public API ────────────────────────────────────────────────────────

    /// <summary>Vuruş anında çağır — particle + ses tetikler.</summary>
    public void PlayHitEffect()
    {
        PlayParticle();
        PlayHitSound();

        if (hitParticlePrefab == null && !flashing)
            StartCoroutine(ColorFlash());
    }

    /// <summary>Hasar miktarına göre efekt yoğunluğu ayarla (opsiyonel).</summary>
    public void PlayHitEffect(float damageAmount, float maxDamage = 100f)
    {
        PlayParticle(damageAmount / maxDamage);
        PlayHitSound();

        if (hitParticlePrefab == null && !flashing)
            StartCoroutine(ColorFlash());
    }

    // ── Particle ──────────────────────────────────────────────────────────

    void PlayParticle(float intensity = 1f)
    {
        if (hitParticlePrefab == null) return;

        Vector3 spawnPos = transform.position + spawnOffset;
        ParticleSystem ps = Instantiate(hitParticlePrefab, spawnPos, Quaternion.identity);

        // Yoğunluğa göre particle sayısını ölçekle
        var main = ps.main;
        main.startSpeedMultiplier = Mathf.Lerp(0.5f, 2f, intensity);

        ps.Play();
        Destroy(ps.gameObject, ps.main.duration + ps.main.startLifetime.constantMax);
    }

    // ── Ses ───────────────────────────────────────────────────────────────

    void PlayHitSound()
    {
        if (hitSounds == null || hitSounds.Length == 0) return;

        AudioClip clip = hitSounds[Random.Range(0, hitSounds.Length)];
        if (clip == null) return;

        if (audioSource != null)
        {
            audioSource.pitch = 1f + Random.Range(-pitchVariance, pitchVariance);
            audioSource.PlayOneShot(clip, hitVolume);
        }
        else
        {
            AudioSource.PlayClipAtPoint(clip, transform.position + spawnOffset, hitVolume);
        }
    }

    // ── Color Flash (Fallback) ─────────────────────────────────────────────

    IEnumerator ColorFlash()
    {
        flashing = true;
        SetColor(flashColor);
        yield return new WaitForSeconds(flashDuration);
        RestoreOriginalColors();
        flashing = false;
    }

    void CacheOriginalColors()
    {
        if (renderers == null) return;
        originalColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null) continue;
            var mat = renderers[i].material;
            originalColors[i] = mat.HasProperty("_Color") ? mat.color : Color.white;
        }
    }

    void SetColor(Color c)
    {
        foreach (var r in renderers)
            if (r != null)
                foreach (var mat in r.materials)
                    if (mat.HasProperty("_Color")) mat.color = c;
    }

    void RestoreOriginalColors()
    {
        if (renderers == null || originalColors == null) return;
        for (int i = 0; i < renderers.Length && i < originalColors.Length; i++)
        {
            if (renderers[i] == null) continue;
            foreach (var mat in renderers[i].materials)
                if (mat.HasProperty("_Color")) mat.color = originalColors[i];
        }
    }
}
