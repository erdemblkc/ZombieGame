using UnityEngine;
using DG.Tweening;

public class WaveCompleteManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject panel;

    [Header("Spawner")]
    public ZombieSpawner1 spawner;

    [Header("Animation")]
    public float fadeInDuration = 0.4f;

    CanvasGroup _cg;

    void Start()
    {
        if (panel != null)
        {
            panel.SetActive(false);
            _cg = panel.GetComponent<CanvasGroup>();
            if (_cg == null) _cg = panel.AddComponent<CanvasGroup>();
        }
    }

    public void ShowPanel()
    {
        if (panel == null)
        {
            Debug.LogError("[WaveCompleteManager] Panel reference is NULL!");
            return;
        }

        Debug.Log("[WaveCompleteManager] ShowPanel()");
        Time.timeScale = 0f;
        panel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        _cg.alpha = 0f;
        panel.transform.localScale = Vector3.one * 0.85f;

        DOTween.Sequence()
            .SetUpdate(true)
            .Append(_cg.DOFade(1f, fadeInDuration).SetEase(Ease.OutQuad))
            .Join(panel.transform.DOScale(Vector3.one, fadeInDuration).SetEase(Ease.OutBack));
    }

    public void OnUpgradeSelected()
    {
        Debug.Log("[WaveCompleteManager] OnUpgradeSelected()");

        if (panel != null)
        {
            DOTween.Kill(_cg);
            _cg.DOFade(0f, 0.2f)
                .SetUpdate(true)
                .OnComplete(() => panel.SetActive(false));
        }

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (spawner == null)
        {
            Debug.LogError("[WaveCompleteManager] Spawner reference is NULL! Inspector'dan ZombieSpawner1 olan objeyi ata.");
            return;
        }

        spawner.SpawnNextWaveFromUI();
    }
}
