using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class WeaponUnlockToast : MonoBehaviour
{
    public CanvasGroup cg;
    public RectTransform root;
    public TextMeshProUGUI titleTMP;
    public Image gunImage;

    [Header("Timing")]
    public float showTime = 1.5f;
    public float fadeOutTime = 0.35f;

    [Header("Motion")]
    public float risePixels = 40f;
    public float startScale = 1.05f;

    void Awake()
    {
        if (cg == null) cg = GetComponent<CanvasGroup>();
        if (root == null) root = GetComponent<RectTransform>();

        cg.alpha = 0f;
        gameObject.SetActive(false);
    }

    public void Show(Sprite weaponSprite, string title = "NEW WEAPON")
    {
        DOTween.Kill(root);
        DOTween.Kill(cg);

        gameObject.SetActive(true);

        if (titleTMP != null) titleTMP.text = title;
        if (gunImage != null)
        {
            gunImage.sprite = weaponSprite;
            gunImage.enabled = (weaponSprite != null);
        }

        Vector2 startPos = root.anchoredPosition;

        cg.alpha = 1f;
        root.localScale = Vector3.one * startScale;

        DOTween.Sequence()
            .SetUpdate(true)
            .Append(root.DOScale(Vector3.one, 0.08f).SetEase(Ease.OutQuad))
            .AppendInterval(showTime)
            .Append(cg.DOFade(0f, fadeOutTime).SetEase(Ease.InQuad))
            .Join(root.DOAnchorPosY(startPos.y + risePixels, fadeOutTime).SetEase(Ease.InQuad))
            .OnComplete(() =>
            {
                root.anchoredPosition = startPos;
                gameObject.SetActive(false);
            });
    }
}
