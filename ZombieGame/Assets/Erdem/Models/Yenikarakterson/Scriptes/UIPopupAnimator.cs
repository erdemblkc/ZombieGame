using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class UIPopupAnimator : MonoBehaviour
{
    [Header("Show Animation")]
    public float showDuration = 0.18f;
    public float startScale = 0.85f;

    [Header("Auto Hide")]
    public bool autoHide = false;
    public float hideDelay = 2f;
    public float hideDuration = 0.15f;

    CanvasGroup cg;
    RectTransform rt;
    Coroutine routine;

    void Awake()
    {
        cg = GetComponent<CanvasGroup>();
        rt = GetComponent<RectTransform>();
    }

    void OnEnable()
    {
        // Her enable olduðunda animasyonu yeniden baþlat
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(ShowRoutine());
    }

    System.Collections.IEnumerator ShowRoutine()
    {
        // baþlangýç
        cg.alpha = 0f;
        rt.localScale = Vector3.one * startScale;

        float t = 0f;
        while (t < showDuration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / showDuration);

            // yumuþak geçiþ
            float eased = 1f - Mathf.Pow(1f - k, 3f);

            cg.alpha = eased;
            rt.localScale = Vector3.Lerp(Vector3.one * startScale, Vector3.one, eased);
            yield return null;
        }

        cg.alpha = 1f;
        rt.localScale = Vector3.one;

        if (autoHide)
        {
            yield return new WaitForSecondsRealtime(hideDelay);
            yield return HideRoutine();
        }
    }

    public void PlayHide()
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(HideRoutine());
    }

    System.Collections.IEnumerator HideRoutine()
    {
        float startA = cg.alpha;
        Vector3 startS = rt.localScale;

        float t = 0f;
        while (t < hideDuration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / hideDuration);
            float eased = Mathf.Pow(k, 3f);

            cg.alpha = Mathf.Lerp(startA, 0f, eased);
            rt.localScale = Vector3.Lerp(startS, Vector3.one * 0.9f, eased);
            yield return null;
        }

        cg.alpha = 0f;
        gameObject.SetActive(false);
    }
}
