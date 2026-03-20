using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class GameOverManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject gameOverPanel; // Siyah panel + yazý + buton içeren ana obje
    public CanvasGroup alphaGroup;   // Panelin þeffaflýðýný kontrol etmek için

    [Header("Settings")]
    public float fadeDuration = 2.0f;

    void Awake()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
            if (alphaGroup != null) alphaGroup.alpha = 0f;
        }
    }

    public void ShowDeathScreen()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            StartCoroutine(FadeRoutine());
        }

        // Mouse'u serbest býrak
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    IEnumerator FadeRoutine()
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime; // Time.timeScale 0 olsa bile çalýþsýn diye
            if (alphaGroup != null)
                alphaGroup.alpha = Mathf.Clamp01(t / fadeDuration);
            yield return null;
        }
        if (alphaGroup != null) alphaGroup.alpha = 1f;
    }

    // Butona baðlayacaðýn fonksiyon
    public void RestartGame()
    {
        // Þu anki sahneyi yeniden yükle
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}