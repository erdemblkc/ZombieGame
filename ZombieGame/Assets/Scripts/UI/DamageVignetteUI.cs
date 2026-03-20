using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DamageVignetteUI : MonoBehaviour
{
    public static DamageVignetteUI Instance;

    [Header("Refs")]
    public Image damageFrame;

    [Header("Effect")]
    [Range(0f, 1f)] public float maxAlpha = 0.45f;
    public float fadeIn = 0.05f;
    public float hold = 0.06f;
    public float fadeOut = 0.18f;

    Coroutine routine;

    void Awake()
    {
        Instance = this;

        if (damageFrame != null)
        {
            var c = damageFrame.color;
            c.a = 0f;
            damageFrame.color = c;
            damageFrame.raycastTarget = false;
        }
    }

    public void Play()
    {
        if (damageFrame == null) return;

        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(PlayRoutine());
    }

    IEnumerator PlayRoutine()
    {
        // Fade In
        yield return FadeTo(maxAlpha, fadeIn);

        // Hold
        if (hold > 0f) yield return new WaitForSeconds(hold);

        // Fade Out
        yield return FadeTo(0f, fadeOut);

        routine = null;
    }

    IEnumerator FadeTo(float targetA, float duration)
    {
        float startA = damageFrame.color.a;
        if (duration <= 0f)
        {
            SetAlpha(targetA);
            yield break;
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(startA, targetA, Mathf.Clamp01(t / duration));
            SetAlpha(a);
            yield return null;
        }

        SetAlpha(targetA);
    }

    void SetAlpha(float a)
    {
        var c = damageFrame.color;
        c.a = a;
        damageFrame.color = c;
    }
}
