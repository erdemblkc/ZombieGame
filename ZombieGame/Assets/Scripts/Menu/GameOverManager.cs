using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class GameOverManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject gameOverPanel;
    public CanvasGroup alphaGroup;

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

            if (alphaGroup != null)
            {
                alphaGroup.alpha = 0f;
                alphaGroup.DOFade(1f, fadeDuration)
                    .SetEase(Ease.InQuad)
                    .SetUpdate(true); // Time.timeScale 0 olsa bile çalışsın
            }
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
