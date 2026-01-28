using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MainMenuController : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mainMenuPanel;
    public GameObject optionsPanel;

    [Header("Gameplay Scripts To Toggle (drag from Player)")]
    public PlayerController2 playerController;
    public GunShooter gunShooter;

    [Header("Gameplay UI Root (HUD)")]
    public GameObject gameplayUIRoot; // GameplayUIRoot buraya

    [Header("Fade")]
    public Image fadeImage;           // FadeImage'in Image component'i
    public float fadeDuration = 1f;   // 1s fade out + 1s fade in = toplam 2s

    [Header("Cinematic")]
    public Camera cinematicCam;
    public Camera gameplayCam;
    public float cinematicDuration = 15f;
    public Animator cinematicAnimator;

    [Header("Cursor")]
    public bool lockCursorInGame = true;

    void Start()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (optionsPanel != null) optionsPanel.SetActive(false);

        // Menüde oyun kontrolü kapalı
        SetGameplayEnabled(false);

        // Menüde HUD kapalı
        if (gameplayUIRoot != null) gameplayUIRoot.SetActive(false);

        // Menüde cursor serbest
        SetCursorMenuMode();

        // Başlangıçta kamera durumu (şimdilik cinematic aktif kalsın)
        if (cinematicCam != null) cinematicCam.enabled = true;
        if (gameplayCam != null) gameplayCam.enabled = false;

        // Fade başlangıç: şeffaf
        SetFadeAlpha(0f);
    }

    public void OnPlayClicked()
    {
        // Menü UI kapanabilir (istersen sadece butonları kapat)
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);

        // Cinematic kamerayı aç, gameplay kamerayı kapat
        if (cinematicCam != null) cinematicCam.enabled = true;
        if (gameplayCam != null) gameplayCam.enabled = false;

        // Animasyonu BAŞTAN oynat
        if (cinematicAnimator != null)
            cinematicAnimator.Play(0, 0, 0f);
    }


    private IEnumerator PlayCinematicThenStartGame()
    {
        // Cinematic ON, Gameplay OFF
        if (cinematicCam != null) cinematicCam.enabled = true;
        if (gameplayCam != null) gameplayCam.enabled = false;

        // Animasyonu baştan oynat
        if (cinematicAnimator != null)
            cinematicAnimator.Play(0, 0, 0f);

        // Cinematic bekle
        yield return new WaitForSeconds(cinematicDuration);

        // 1) Fade to black (1 sn)
        yield return Fade(0f, 1f, fadeDuration);

        // 2) Kamera değiştir
        if (cinematicCam != null) cinematicCam.enabled = false;
        if (gameplayCam != null) gameplayCam.enabled = true;

        // 3) HUD + kontrol aç
        if (gameplayUIRoot != null) gameplayUIRoot.SetActive(true);
        SetGameplayEnabled(true);

        // 4) Fade from black (1 sn)
        yield return Fade(1f, 0f, fadeDuration);
    }

    void SetGameplayEnabled(bool enabled)
    {
        if (playerController != null) playerController.enabled = enabled;
        if (gunShooter != null) gunShooter.enabled = enabled;
    }

    void SetCursorMenuMode()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void SetCursorGameMode()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    IEnumerator Fade(float from, float to, float duration)
    {
        if (fadeImage == null) yield break;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(from, to, t / duration);
            SetFadeAlpha(a);
            yield return null;
        }
        SetFadeAlpha(to);
    }

    void SetFadeAlpha(float a)
    {
        if (fadeImage == null) return;
        Color c = fadeImage.color;
        c.a = a;
        fadeImage.color = c;
    }
}
