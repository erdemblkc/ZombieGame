using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

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

        damageFrame.DOKill();

        DOTween.Sequence()
            .Append(damageFrame.DOFade(maxAlpha, fadeIn).SetEase(Ease.OutQuad))
            .AppendInterval(hold)
            .Append(damageFrame.DOFade(0f, fadeOut).SetEase(Ease.InQuad));
    }
}
