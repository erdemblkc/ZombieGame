using UnityEngine;

public class MenuOnly : MonoBehaviour
{
    public GameObject menuRoot;          // MainMenuPanel
    public Animator cinematicAnimator;   // CinematicCam Animator

    public void OnPlay()
    {
        if (menuRoot != null)
            menuRoot.SetActive(false);

        if (cinematicAnimator != null)
            cinematicAnimator.Play("Cinematic Cam", 0, 0f); // clip state adýn buysa
        // Eðer state adý farklýysa: Animator penceresindeki state adýný buraya yaz.
    }
}
