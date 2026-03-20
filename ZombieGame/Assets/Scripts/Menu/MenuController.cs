using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [Header("UI")]
    public GameObject menuRoot;     // Play+Options'ýn bulunduðu panel (veya tüm canvas grubu)
    public Button playButton;

    [Header("Camera Animation")]
    public Animator cameraAnimator; // Kameradaki Animator
    public float introDuration = 15f;

    [Header("Disable Gameplay Until Play")]
    public Behaviour[] disableOnStart; // PlayerController, MouseLook vs scriptlerini buraya sürükle

    void Start()
    {
        // Menü açýk
        if (menuRoot) menuRoot.SetActive(true);

        // Mouse serbest ve görünür
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Gameplay scriptlerini kapat (karakter/kamera dönmesin)
        if (disableOnStart != null)
        {
            foreach (var b in disableOnStart)
                if (b) b.enabled = false;
        }

        // Play button click baðla
        if (playButton)
        {
            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(OnPlayPressed);
        }
    }

    public void OnPlayPressed()
    {
        // Buton spam olmasýn
        if (playButton) playButton.interactable = false;

        // Menü gizle
        if (menuRoot) menuRoot.SetActive(false);

        // Kamera animasyonu baþlat
        if (cameraAnimator) cameraAnimator.SetTrigger("PlayIntro");

        StartCoroutine(EnableGameplayAfterIntro());
    }

    IEnumerator EnableGameplayAfterIntro()
    {
        yield return new WaitForSeconds(introDuration);

        // Ýstersen burada gameplay’i aç
        if (disableOnStart != null)
        {
            foreach (var b in disableOnStart)
                if (b) b.enabled = true;
        }

        // Ýstersen mouse’u kilitle (FPS ise)
        // Cursor.visible = false;
        // Cursor.lockState = CursorLockMode.Locked;
    }
}
