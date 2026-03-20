using System.Collections;
using UnityEngine;

public class PausePopupController : MonoBehaviour
{
    [Header("Auto Hide")]
    public float showSeconds = 1.0f;

    Coroutine co;

    void OnEnable()
    {
        // OYUN DURMASIN
        Time.timeScale = 1f;

        // Mouse kilidine dokunmayalým (FPS oynanýþýn bozulmasýn)
        // Cursor.lockState / visible AYARLAMiyoruz.

        if (co != null) StopCoroutine(co);
        co = StartCoroutine(AutoHide());
    }

    IEnumerator AutoHide()
    {
        yield return new WaitForSeconds(showSeconds);
        gameObject.SetActive(false);
    }
}
