using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Kapıya trigger collider ekle, bu scripti koy.
/// Wave bitmeden kapı çalışmaz.
/// Geçiş: ekran kararır → sonraki sahne yüklenir veya hedef pozisyona ışınlanır.
/// </summary>
public class RoomDoorTrigger : MonoBehaviour
{
    public enum TransitionType { LoadScene, Teleport }

    [Header("Geçiş Tipi")]
    public TransitionType transitionType = TransitionType.Teleport;

    [Header("Load Scene (transitionType = LoadScene)")]
    public string targetSceneName;

    [Header("Teleport (transitionType = Teleport)")]
    [Tooltip("Player bu noktaya ışınlanır.")]
    public Transform teleportTarget;

    [Header("Fade")]
    public float fadeDuration = 1f;

    [Header("Oda İsmi (opsiyonel)")]
    [Tooltip("Geçiş sonrası ekranda gösterilecek oda ismi. Boş bırakılırsa gösterilmez.")]
    public string roomName;

    [Header("Prompt (opsiyonel)")]
    [Tooltip("Wave bitmeden önce gösterilecek mesaj canvas'ı (null bırakılabilir).")]
    public GameObject lockedPrompt;

    // Runtime
    private Image  _fadeImage;
    private bool   _transitioning;

    // ── Lifecycle ──────────────────────────────────────────────────────────

    void Awake()
    {
        // Fade için siyah tam ekran Image oluştur
        var canvasGO = new GameObject("DoorFadeCanvas");
        var canvas   = canvasGO.AddComponent<Canvas>();
        canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;
        canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        var imgGO = new GameObject("FadeImage");
        imgGO.transform.SetParent(canvasGO.transform, false);
        _fadeImage = imgGO.AddComponent<Image>();
        _fadeImage.color = new Color(0f, 0f, 0f, 0f);
        _fadeImage.raycastTarget = false;

        var rect = imgGO.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        DontDestroyOnLoad(canvasGO);
    }

    // ── Trigger ───────────────────────────────────────────────────────────

    void OnTriggerEnter(Collider other)
    {
        if (_transitioning) return;
        if (!other.CompareTag("Player")) return;

        // Wave hâlâ devam ediyorsa engelle
        if (WaveManager.Instance != null && WaveManager.Instance.WaveActive)
        {
            if (lockedPrompt != null)
                StartCoroutine(ShowPromptBriefly());
            return;
        }

        StartCoroutine(DoTransition(other.transform.root));
    }

    // ── Routines ──────────────────────────────────────────────────────────

    IEnumerator DoTransition(Transform player)
    {
        _transitioning = true;

        // Ekranı karart
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            _fadeImage.color = new Color(0f, 0f, 0f, Mathf.Clamp01(t / fadeDuration));
            yield return null;
        }
        _fadeImage.color = Color.black;

        // Geçiş yap
        if (transitionType == TransitionType.LoadScene && !string.IsNullOrEmpty(targetSceneName))
        {
            SceneManager.LoadScene(targetSceneName);
        }
        else if (transitionType == TransitionType.Teleport && teleportTarget != null)
        {
            // CharacterController varsa kapat, ışınla, aç
            var cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            player.position = teleportTarget.position;
            if (cc != null) cc.enabled = true;
        }

        // Ekranı aç
        yield return new WaitForSeconds(0.2f);
        t = fadeDuration;
        while (t > 0f)
        {
            t -= Time.deltaTime;
            _fadeImage.color = new Color(0f, 0f, 0f, Mathf.Clamp01(t / fadeDuration));
            yield return null;
        }
        _fadeImage.color = new Color(0f, 0f, 0f, 0f);

        // Oda ismini göster
        if (!string.IsNullOrEmpty(roomName))
        {
            var waveHUD = FindFirstObjectByType<WaveHUD>();
            waveHUD?.ShowRoomName(roomName);
        }

        _transitioning = false;
    }

    IEnumerator ShowPromptBriefly()
    {
        if (lockedPrompt == null) yield break;
        lockedPrompt.SetActive(true);
        yield return new WaitForSeconds(2f);
        lockedPrompt.SetActive(false);
    }
}
