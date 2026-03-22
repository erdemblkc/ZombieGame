using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

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

    // VURUNCA ÇAĞIR
    public void OnHit()
    {
        PlayCrosshairPop();
        PlayHitmarker();
    }

    void PlayCrosshairPop()
    {
        if (crosshair == null) return;

        crosshair.rectTransform.DOKill();
        crosshair.rectTransform.localScale = Vector3.one;
        crosshair.rectTransform.DOPunchScale(Vector3.one * (popScale - 1f), popTime, 1, 0.5f);
    }

    void PlayHitmarker()
    {
        if (hitMarker == null) return;

        hitMarker.DOKill();
        hitMarker.gameObject.SetActive(true);
        hitMarker.DOFade(1f, 0f)
            .OnComplete(() =>
                hitMarker.DOFade(0f, hitFadeOut)
                    .SetDelay(hitShowTime)
                    .SetEase(Ease.InQuad)
                    .OnComplete(() => hitMarker.gameObject.SetActive(false))
            );
    }
}
