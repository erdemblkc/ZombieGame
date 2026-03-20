using TMPro;
using UnityEngine;
using DG.Tweening;

public class InteractPromptUI : MonoBehaviour
{
    public static InteractPromptUI Instance;

    public TextMeshProUGUI promptText;
    public GameObject root;

    [Header("Animation")]
    public float fadeDuration = 0.15f;

    CanvasGroup _cg;

    void Awake()
    {
        Instance = this;

        if (root != null)
        {
            _cg = root.GetComponent<CanvasGroup>();
            if (_cg == null) _cg = root.AddComponent<CanvasGroup>();
            _cg.alpha = 0f;
            root.SetActive(false);
        }
    }

    public void Show(string text)
    {
        if (root == null) return;
        if (promptText != null) promptText.text = text;

        _cg.DOKill();
        root.SetActive(true);
        root.transform.DOKill();
        root.transform.localScale = Vector3.one * 0.9f;
        root.transform.DOScale(Vector3.one, fadeDuration).SetEase(Ease.OutBack);
        _cg.DOFade(1f, fadeDuration);
    }

    public void Hide()
    {
        if (root == null) return;

        _cg.DOKill();
        root.transform.DOKill();
        _cg.DOFade(0f, fadeDuration)
            .OnComplete(() => root.SetActive(false));
    }
}
