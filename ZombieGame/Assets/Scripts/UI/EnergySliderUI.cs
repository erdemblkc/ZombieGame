using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class EnergySliderUI : MonoBehaviour
{
    [Header("Refs")]
    public PlayerController2 player;
    public Slider energySlider;

    [Header("Smooth")]
    public float tweenDuration = 0.08f;

    float _target01 = 1f;
    float _lastTarget = -1f;

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
        _lastTarget = _target01;

        if (energySlider != null)
            energySlider.value = _target01;
    }

    void Update()
    {
        if (player == null || energySlider == null) return;

        UpdateTarget();

        if (Mathf.Abs(_target01 - _lastTarget) > 0.001f)
        {
            _lastTarget = _target01;
            energySlider.DOKill();
            energySlider.DOValue(_target01, tweenDuration).SetEase(Ease.OutQuad);
        }
    }

    void UpdateTarget()
    {
        float max = Mathf.Max(0.0001f, player.MaxEnergy);
        _target01 = Mathf.Clamp01(player.CurrentEnergy / max);
    }
}
