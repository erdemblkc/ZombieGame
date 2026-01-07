using UnityEngine;
using UnityEngine.UI;

public class EnergyBarUI : MonoBehaviour
{
    public PlayerController2 player;
    public Image fillImage;

    void Awake()
    {
        if (player == null)
            player = FindObjectOfType<PlayerController2>();
    }

    void Update()
    {
        if (player == null || fillImage == null) return;

        float max = Mathf.Max(0.0001f, player.MaxEnergy);
        float t = Mathf.Clamp01(player.CurrentEnergy / max);
        fillImage.fillAmount = t;
    }
}
