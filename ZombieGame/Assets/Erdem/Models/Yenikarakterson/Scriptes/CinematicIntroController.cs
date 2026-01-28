using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CinematicIntroController : MonoBehaviour
{
    [Header("Menu")]
    public GameObject mainMenuPanel;

    [Header("Cameras")]
    public Camera cinematicCam;
    public Camera gameplayCam;

    [Header("Cinematic")]
    public Animator cinematicAnimator;
    public float cinematicDuration = 15f; // 15 sn

    [Header("Fade")]
    public Image fadeImage;
    public float fadeDuration = 1f; // 1 sn out + 1 sn in

    public void PlayIntro()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        StartCoroutine(IntroRoutine());
    }

    IEnumerator IntroRoutine()
    {
        // Cinematic cam ON, gameplay OFF
        if (cinematicCam != null) cinematicCam.enabled = true;
        if (gameplayCam != null) gameplayCam.enabled = false;

        // Fade þeffaf baþlasýn
        SetFadeAlpha(0f);

        // Animasyonu baþtan oynat
        if (cinematicAnimator != null)
            cinematicAnimator.Play(0, 0, 0f);

        // 15 saniye bekle
        yield return new WaitForSeconds(cinematicDuration);

        // Fade to black (1 sn)
        yield return Fade(0f, 1f, fadeDuration);

        // Kamera deðiþtir
        if (cinematicCam != null) cinematicCam.enabled = false;
        if (gameplayCam != null) gameplayCam.enabled = true;

        // Fade from black (1 sn)
        yield return Fade(1f, 0f, fadeDuration);
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
