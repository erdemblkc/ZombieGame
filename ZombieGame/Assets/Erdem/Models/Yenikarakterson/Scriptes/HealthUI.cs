using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    public PlayerDamageReceiver player;
    public Slider healthSlider;

    public bool hideWhenFull = false;
    public float smoothSpeed = 12f;

    float _target01 = 1f;

    void Awake()
    {
        if (healthSlider != null)
        {
            healthSlider.minValue = 0f;
            healthSlider.maxValue = 1f;
        }
    }

    void Start()
    {
        if (player == null)
            player = FindFirstObjectByType<PlayerDamageReceiver>();

        if (player != null)
            _target01 = (player.maxHealth <= 0f) ? 1f : (player.currentHealth / player.maxHealth);
    }

    void Update()
    {
        if (healthSlider == null || player == null) return;

        _target01 = (player.maxHealth <= 0f) ? 1f : Mathf.Clamp01(player.currentHealth / player.maxHealth);
        healthSlider.value = Mathf.Lerp(healthSlider.value, _target01, smoothSpeed * Time.deltaTime);

        if (hideWhenFull)
        {
            bool show = healthSlider.value < 0.999f;
            if (healthSlider.gameObject.activeSelf != show)
                healthSlider.gameObject.SetActive(show);
        }
    }
}
