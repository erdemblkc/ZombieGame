using UnityEngine;
using UnityEngine.UI;

public class EnergySliderUI : MonoBehaviour
{
    [Header("Refs")]
    public PlayerController2 player;
    public Slider energySlider;

    [Header("Smooth")]
    public float smoothSpeed = 12f;

    float _target01 = 1f;

    void Awake()
    {
        if (energySlider != null)
        {
            energySlider.minValue = 0f;
            energySlider.maxValue = 1f;
        }
    }

    void Start()
    {
        if (player == null)
            player = FindFirstObjectByType<PlayerController2>();

        UpdateTarget();

        if (energySlider != null)
            energySlider.value = _target01;
    }

    void Update()
    {
        if (player == null || energySlider == null) return;

        UpdateTarget();

        energySlider.value =
            Mathf.Lerp(energySlider.value, _target01, smoothSpeed * Time.deltaTime);
    }

    void UpdateTarget()
    {
        float max = Mathf.Max(0.0001f, player.MaxEnergy);
        _target01 = Mathf.Clamp01(player.CurrentEnergy / max);
    }
}
