using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class MenuController : MonoBehaviour
{
    [Header("UI")]
    public GameObject menuRoot;
    public Button playButton;

    [Header("Camera Animation")]
    public Animator cameraAnimator;
    public float introDuration = 15f;

    [Header("Disable Gameplay Until Play")]
    public Behaviour[] disableOnStart;

    [Header("Animation")]
    public float menuFadeInDuration = 0.4f;
    public float menuFadeOutDuration = 0.3f;

    CanvasGroup _menuCG;

    void Start()
    {
        if (menuRoot)
        {
            menuRoot.SetActive(true);
            _menuCG = menuRoot.GetComponent<CanvasGroup>();
            if (_menuCG == null) _menuCG = menuRoot.AddComponent<CanvasGroup>();

            _menuCG.alpha = 0f;
            _menuCG.DOFade(1f, menuFadeInDuration).SetEase(Ease.OutQuad);
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (disableOnStart != null)
            foreach (var b in disableOnStart)
                if (b) b.enabled = false;

        if (playButton)
        {
            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(OnPlayPressed);
        }
    }

    public void OnPlayPressed()
    {
        if (playButton) playButton.interactable = false;

        if (_menuCG != null)
        {
            _menuCG.DOFade(0f, menuFadeOutDuration)
                .SetEase(Ease.InQuad)
                .OnComplete(() => menuRoot.SetActive(false));
        }
        else if (menuRoot)
        {
            menuRoot.SetActive(false);
        }

        if (cameraAnimator) cameraAnimator.SetTrigger("PlayIntro");

        DOVirtual.DelayedCall(introDuration, EnableGameplay);
    }

    void EnableGameplay()
    {
        if (disableOnStart != null)
            foreach (var b in disableOnStart)
                if (b) b.enabled = true;
    }
}
