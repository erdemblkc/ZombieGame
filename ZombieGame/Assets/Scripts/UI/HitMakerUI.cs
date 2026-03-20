using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HitmarkerUI : MonoBehaviour
{
    public static HitmarkerUI Instance;

    [Header("Refs")]
    public Image crosshair;
    public Image hitMarker;

    [Header("Crosshair Pop")]
    public float popScale = 1.18f;
    public float popTime = 0.08f;

    [Header("Hitmarker")]
    public float hitShowTime = 0.06f;
    public float hitFadeOut = 0.10f;

    Coroutine popCo;
    Coroutine hitCo;

    void Awake()
    {
        Instance = this;

        if (hitMarker != null)
        {
            hitMarker.gameObject.SetActive(false);
            var c = hitMarker.color;
            c.a = 1f;
            hitMarker.color = c;
        }
    }

    // VURUNCA ÇAÐIR
    public void OnHit()
    {
        PlayCrosshairPop();
        PlayHitmarker();
    }

    void PlayCrosshairPop()
    {
        if (crosshair == null) return;

        if (popCo != null) StopCoroutine(popCo);
        popCo = StartCoroutine(PopRoutine(crosshair.rectTransform));
    }

    IEnumerator PopRoutine(RectTransform rt)
    {
        Vector3 baseScale = Vector3.one;
        rt.localScale = baseScale;

        // hýzlý büyüt
        rt.localScale = baseScale * popScale;
        yield return new WaitForSeconds(popTime * 0.5f);

        // geri dön
        float t = 0f;
        float dur = popTime * 0.5f;
        while (t < dur)
        {
            t += Time.deltaTime;
            float a = Mathf.Clamp01(t / dur);
            rt.localScale = Vector3.Lerp(baseScale * popScale, baseScale, a);
            yield return null;
        }
        rt.localScale = baseScale;
        popCo = null;
    }

    void PlayHitmarker()
    {
        if (hitMarker == null) return;

        if (hitCo != null) StopCoroutine(hitCo);
        hitCo = StartCoroutine(HitRoutine());
    }

    IEnumerator HitRoutine()
    {
        hitMarker.gameObject.SetActive(true);

        // reset alpha
        var c = hitMarker.color;
        c.a = 1f;
        hitMarker.color = c;

        // kýsa süre göster
        yield return new WaitForSeconds(hitShowTime);

        // fade out
        float t = 0f;
        while (t < hitFadeOut)
        {
            t += Time.deltaTime;
            float a = 1f - Mathf.Clamp01(t / hitFadeOut);
            c.a = a;
            hitMarker.color = c;
            yield return null;
        }

        hitMarker.gameObject.SetActive(false);
        hitCo = null;
    }
}
