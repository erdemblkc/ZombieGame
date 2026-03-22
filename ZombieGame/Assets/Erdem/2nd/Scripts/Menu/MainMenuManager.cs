using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using TMPro;

/// <summary>
/// Ana menü yöneticisi.
/// Start → GameScene yükler | Options → panel slide | Quit → çıkış | Sound → mute toggle
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    [Header("Scene")]
    public string gameSceneName = "GameScene";

    [Header("Ana Paneller")]
    public CanvasGroup mainPanel;       // Tüm menü container'ı
    public CanvasGroup fadeOverlay;     // Geçiş için siyah overlay

    [Header("Options Paneli")]
    public RectTransform optionsPanel;
    public CanvasGroup   optionsCG;

    [Header("Butonlar")]
    public Button startButton;
    public Button optionsButton;
    public Button quitButton;
    public Button soundToggleButton;
    public Button optionsCloseButton;

    [Header("Ses İkonu")]
    public Image   soundIconImage;     // Ses butonundaki ikon
    public Color   soundOnColor  = Color.white;
    public Color   soundOffColor = new Color(0.4f, 0.4f, 0.4f);

    [Header("Options İçeriği")]
    public Slider masterVolumeSlider;
    public TextMeshProUGUI volumeValueText;

    [Header("Animasyon")]
    public float fadeInDuration  = 0.5f;
    public float fadeOutDuration = 0.7f;

    // ─────────────────────────────────────────────────────────────────────────

    private bool    _soundOn     = true;
    private bool    _optionsOpen = false;
    private Vector2 _optionsShownPos;
    private Vector2 _optionsHiddenPos;

    // ═════════════════════════════════════════════════════════════════════════

    private void Start()
    {
        Cursor.visible   = true;
        Cursor.lockState = CursorLockMode.None;

        SetupFade();
        SetupOptionsPanel();
        SetupVolumeSlider();
        BindButtons();
    }

    // ── Setup ─────────────────────────────────────────────────────────────────

    private void SetupFade()
    {
        // Sahne açılırken: overlay siyah → şeffaf
        if (fadeOverlay != null)
        {
            fadeOverlay.alpha          = 1f;
            fadeOverlay.blocksRaycasts = false;
            fadeOverlay.DOFade(0f, fadeInDuration).SetEase(Ease.OutQuad);
        }

        // Ana panel fade in
        if (mainPanel != null)
        {
            mainPanel.alpha = 0f;
            mainPanel.DOFade(1f, fadeInDuration).SetEase(Ease.OutQuad).SetDelay(0.1f);
        }
    }

    private void SetupOptionsPanel()
    {
        if (optionsPanel == null) return;

        _optionsShownPos = optionsPanel.anchoredPosition;
        // Ekranın altına gizle
        _optionsHiddenPos = _optionsShownPos + new Vector2(0f, -700f);
        optionsPanel.anchoredPosition = _optionsHiddenPos;

        if (optionsCG != null)
        {
            optionsCG.alpha          = 0f;
            optionsCG.blocksRaycasts = false;
        }
    }

    private void SetupVolumeSlider()
    {
        if (masterVolumeSlider == null) return;

        masterVolumeSlider.value = AudioListener.volume;
        masterVolumeSlider.onValueChanged.AddListener(v =>
        {
            AudioListener.volume = v;
            if (volumeValueText != null)
                volumeValueText.text = Mathf.RoundToInt(v * 100f) + "%";

            // Slider değeri değişince ses ikonunu güncelle
            _soundOn = v > 0.01f;
            UpdateSoundIcon();
        });

        if (volumeValueText != null)
            volumeValueText.text = Mathf.RoundToInt(AudioListener.volume * 100f) + "%";
    }

    private void BindButtons()
    {
        startButton?.onClick.AddListener(OnStartPressed);
        optionsButton?.onClick.AddListener(OnOptionsPressed);
        quitButton?.onClick.AddListener(OnQuitPressed);
        soundToggleButton?.onClick.AddListener(OnSoundTogglePressed);
        optionsCloseButton?.onClick.AddListener(OnOptionsClosePressed);
    }

    // ── Button Handlers ───────────────────────────────────────────────────────

    public void OnStartPressed()
    {
        if (startButton) startButton.interactable = false;

        if (fadeOverlay != null)
        {
            fadeOverlay.blocksRaycasts = true;
            fadeOverlay.DOFade(1f, fadeOutDuration)
                .SetEase(Ease.InQuad)
                .OnComplete(() => SceneManager.LoadScene(gameSceneName));
        }
        else
        {
            SceneManager.LoadScene(gameSceneName);
        }
    }

    public void OnOptionsPressed()
    {
        if (_optionsOpen) return;
        _optionsOpen = true;

        optionsPanel.gameObject.SetActive(true);
        optionsPanel.DOAnchorPos(_optionsShownPos, 0.38f).SetEase(Ease.OutBack);

        if (optionsCG != null)
        {
            optionsCG.blocksRaycasts = true;
            optionsCG.DOFade(1f, 0.25f);
        }
    }

    public void OnOptionsClosePressed()
    {
        if (!_optionsOpen) return;

        if (optionsCG != null)
        {
            optionsCG.blocksRaycasts = false;
            optionsCG.DOFade(0f, 0.2f);
        }

        optionsPanel.DOAnchorPos(_optionsHiddenPos, 0.28f)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                optionsPanel.gameObject.SetActive(false);
                _optionsOpen = false;
            });
    }

    public void OnQuitPressed()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void OnSoundTogglePressed()
    {
        _soundOn             = !_soundOn;
        AudioListener.volume = _soundOn ? 1f : 0f;

        if (masterVolumeSlider != null)
            masterVolumeSlider.value = AudioListener.volume;

        UpdateSoundIcon();
    }

    private void UpdateSoundIcon()
    {
        if (soundIconImage != null)
            soundIconImage.color = _soundOn ? soundOnColor : soundOffColor;
    }
}
