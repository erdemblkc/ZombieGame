using UnityEngine;
using DG.Tweening;

public class PausePopupController : MonoBehaviour
{
    [Header("Auto Hide")]
    public float showSeconds = 1.0f;

    [Header("Animation")]
    public float fadeDuration = 0.2f;

    CanvasGroup _cg;

    void Awake()
    {
        _cg = GetComponent<CanvasGroup>();
        if (_cg == null) _cg = gameObject.AddComponent<CanvasGroup>();
    }

    void OnEnable()
    {
        // OYUN DURMASIN
        Time.timeScale = 1f;

        _cg.DOKill();
        _cg.alpha = 0f;
        _cg.DOFade(1f, fadeDuration)
            .SetUpdate(true)
            .OnComplete(() =>
                DOVirtual.DelayedCall(showSeconds, HideWithFade, false)
            );
    }

    void HideWithFade()
    {
        _cg.DOKill();
        _cg.DOFade(0f, fadeDuration)
            .SetUpdate(true)
            .OnComplete(() => gameObject.SetActive(false));
    }
}
